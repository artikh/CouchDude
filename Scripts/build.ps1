properties {
    if ($version -eq $null) {
        throw "Version should be declared explictly"
    }    

    $config = 'Debug'
    $isDebug = $conifg -eq 'Debug'
    
    $scriptDir = (resolve-path .).Path
    $rootDir = (resolve-path ..).Path;
    $buildDir = "$rootDir\Build";   
    $srcDir = "$rootDir\Src"; 
}

task default -depends test, package

task clean {
    if(test-path $buildDir) {
        dir $buildDir | del -Recurse -Force
    }
    mkdir -Force $buildDir
}

task setVersion {
  $assemblyInfoFileName = "$srcDir\GlobalAssemblyInfo.cs"
  
  $assembyInfo = [System.IO.File]::ReadAllText($assemblyInfoFileName)
  $assembyInfo = $assembyInfo -replace "Version\((.*)\)]", "Version(`"$version`")]"
  
  $assembyInfo.Trim() > $assemblyInfoFileName
}

task buildCore -depends clean, setVersion {
    exec { msbuild $rootDir\Src\Core\CouchDude.sln /nologo /p:Config=$config /maxcpucount /verbosity:minimal }
}

task buildSchemeManager

task buildBootstrapper -depends clean, setVersion {
    exec { msbuild $rootDir\Src\Bootstrapper\Bootstrapper.sln /nologo /p:Config=$config /maxcpucount /verbosity:minimal }
}

task testCore -depends buildCore {
    exec { ..\Tools\xunit\xunit.console.clr4.x86.exe "$rootDir\Src\Core\Tests\bin\$config\CouchDude.Tests.dll" /silent /-trait "level=integration" /html $buildDir\CouchDude.Tests.html }
}

task test -depends testCore {
    #if test was succesfull deleting protocol
    rm $buildDir\CouchDude.Tests.html
}

function replaceToken([string]$fileName, [string]$tokenName, [string]$tokenValue) {
    $content = (cat $fileName)
    $content | % { $_.Replace($tokenName, $tokenValue) } > $fileName
}

$nuSpecNamespace = 'http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd'

function prepareNuspec([string]$templateFileName, [string]$targetFileName, [array]$fileTemplates) {
    $specXml = [xml](get-content $templateFileName)
    
    $filesNode = $specXml.CreateElement('files', $nuSpecNamespace);
    foreach($fileTemplate in $fileTemplates) {
        foreach($fileName in (resolve-path $fileTemplate | %{ $_.Path } )) {
            $fileNode = $specXml.CreateElement('file', $nuSpecNamespace);
            $fileNode.SetAttribute('src', $fileName);
            $fileNode.SetAttribute('target', 'lib\net40');
            $filesNode.AppendChild($fileNode);
        }
    }
    $specXml.package.AppendChild($filesNode);

    $specXml.Save($targetFileName);
}

function createOrClear($dirName) {
    if((test-path $dirName)) {
        rm -Recurse -Force $dirName 
    }
    mkdir -Force $dirName
    return $dirName
}

function packNuGet([string]$nuspecFile) {
    exec { ..\tools\nuget\NuGet.exe pack $nuspecFile -OutputDirectory $buildDir }    
}

function prepareAndPackage([string]$templateNuSpec, [array]$fileTemplates) {
    $nuspecName = [System.IO.Path]::GetFileNameWithoutExtension($templateNuSpec)
    $nuspecFile = "$buildDir\$nuspecName.nuspec"

    prepareNuspec -template "$templateNuspec" -target $nuspecFile -fileTemplates $fileTemplates
    replaceToken -file $nuspecFile -tokenName '$version$' -tokenValue $version

    packNuGet $nuspecFile

    del $nuspecFile
}

task packageCore -depends buildCore {
    prepareAndPackage -templateNuSpec "$srcDir\Core\Core\CouchDude.nuspec" -fileTemplates ("$srcDir\Core\Core\bin\$config\CouchDude.*")
}

task packageSchemeManager -depends buildSchemeManager {
    prepareAndPackage -templateNuSpec "$srcDir\SchemeManager\CouchDude.SchemeManager.nuspec" -fileTemplates ("$srcDir\SchemeManager\bin\$config\CouchDude.SchemeManager.*")
}

task packageBootstrapper -depends buildBootstrapper {
    prepareAndPackage -templateNuSpec "$srcDir\Bootstrapper\Core\CouchDude.Bootstrapper.nuspec" -fileTemplates ("$srcDir\Bootstrapper\Core\bin\$config\CouchDude.Bootstrapper.*")
}

task packageAzureBootstrapper {
    prepareAndPackage -templateNuSpec "$srcDir\Bootstrapper\Azure\CouchDude.Bootstrapper.Azure.nuspec" `
                      -fileTemplates ("$srcDir\Bootstrapper\Azure\bin\$config\CouchDude.Bootstrapper.Azure.*")
}

task package -depends packageCore, packageBootstrapper, packageAzureBootstrapper #, packageSchemeManager