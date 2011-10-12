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

task default -depends build, test, package

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

task build -depends clean, setVersion {
    exec { msbuild $rootDir\CouchDude.sln /nologo /p:Config=$config /p:Platform="Mixed Platforms" /maxcpucount /verbosity:minimal }
}

task test -depends build {
    exec { ..\Tools\xunit\xunit.console.clr4.x86.exe "$rootDir\Tests\bin\$config\CouchDude.Tests.dll" /silent /-trait "level=integration" /html $buildDir\testResult.html }
}

function replaceToken([string]$fileName, [string]$tokenName, [string]$tokenValue) {
    $content = (cat $fileName)
    $content | % { $_.Replace($tokenName, $tokenValue) } > $fileName
}

function prepareNuspec([string]$templateFileName, [string]$targetFileName, [array]$fileTemplates) {
    $specXml = [xml](get-content $templateFileName)
    
    $filesNode = $specXml.CreateElement('files');
    foreach($fileTemplate in $fileTemplates) {
        foreach($fileName in (resolve-path $fileTemplate | %{ $_.Path } )) {
            $fileNode = $specXml.CreateElement('file');
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

function prepareAndPackage([string]$templateNuSpec, [array]$filesToPack) {
    $nuspecName = [System.IO.Path]::GetFileNameWithoutExtension($templateNuSpec)
    $nuspecFile = "$buildDir\$nuspecName.nuspec"

    prepareNuspec -template "$templateNuspec" `
                  -target $nuspecFile `
                  -files $filesToPack
    replaceToken -file $nuspecFile -tokenName '$version$' -tokenValue $version

    packNuGet $nuspecFile

    del $nuspecFile
}

task packageCore   {
    prepareAndPackage -templateNuSpec "$srcDir\Core\CouchDude.nuspec" -files ("$srcDir\Core\bin\$config\CouchDude.*")
}

task packageSchemeManager -depends build {
    prepareAndPackage -templateNuSpec "$srcDir\SchemeManager\CouchDude.SchemeManager.nuspec" -files ("$srcDir\SchemeManager\bin\$config\CouchDude.SchemeManager.*")
}

task packageBootstrapper -depends build {
    prepareAndPackage -templateNuSpec "$srcDir\Bootstrapper\CouchDude.Bootstrapper.nuspec" -files ("$srcDir\Bootstrapper\bin\$config\CouchDude.Bootstrapper.*")
}

task packageAzureBootstrapper -depends build {
    prepareAndPackage -templateNuSpec "$srcDir\Bootstrapper.Azure\CouchDude.Bootstrapper.Azure.nuspec" `
                      -files ("$srcDir\Bootstrapper.Azure\bin\$config\CouchDude.Azure.Bootstrapper.*")
}

task package -depends packageCore, packageSchemeManager, packageBootstrapper, packageAzureBootstrapper