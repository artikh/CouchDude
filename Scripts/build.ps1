properties {
    Import-Module .\teamcity.psm1

    $config = 'Debug'
    
    $scriptDir = (resolve-path .).Path
    $rootDir = (resolve-path ..).Path;
    $buildDir = "$rootDir\Build";   
    $srcDir = "$rootDir\Src";
    $assemblyInfoFileName = "$rootDir\GlobalAssemblyInfo.cs"

    if ($version -eq $null) {
        $globalAssemblyInfo = (cat $assemblyInfoFileName)

        $match = [regex]::Match($globalAssemblyInfo, '\[assembly: AssemblyVersion\("([\.\d]+)"\)\]')
        $version = $match.Groups[1].Value
    }
    
    if ($nugetSources -eq $null) {
        $nugetSources = "https://go.microsoft.com/fwlink/?LinkID=206669"
    }
}

task default -depends setVersion, test, package

task clean {
    if(test-path $buildDir) {
        dir $buildDir | del -Recurse -Force
    }
    mkdir -Force $buildDir > $null
}

task setVersion {  
    $assembyInfo = [System.IO.File]::ReadAllText($assemblyInfoFileName)
    $assembyInfo = $assembyInfo -replace "Version\((.*)\)]", "Version(`"$version`")]"
    $assembyInfo.Trim() > $assemblyInfoFileName
}


task installPackages {    
    exec { ..\tools\nuget\NuGet.exe install "$rootDir\Src\packages.config" -Source $nugetSources -OutputDirectory "$rootDir\Packages"  }
    exec { ..\tools\nuget\NuGet.exe install "$rootDir\Tests\packages.config" -Source $nugetSources -OutputDirectory "$rootDir\Packages"  }
}

task build -depends clean, installPackages {
    exec { msbuild "$rootDir\CouchDude.sln" /nologo /p:Config=$config /p:Platform='Any Cpu' /maxcpucount /verbosity:minimal }    
}


task test -depends build {
    $outputFileName = "$buildDir\test-results-$version.html"
    exec { ..\Tools\xunit\xunit.console.clr4.x86.exe "$rootDir\Tests\bin\$config\CouchDude.Tests.dll" /silent /-trait "level=integration" /html $outputFileName }
    TeamCity-PublishArtifact $outputFileName
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

    prepareNuspec -template "$templateNuspec" -target $nuspecFile -fileTemplates $fileTemplates > $null
    replaceToken -file $nuspecFile -tokenName '$version$' -tokenValue $version

    packNuGet $nuspecFile

    del $nuspecFile
}

task package -depends build {
    prepareAndPackage -templateNuSpec "$rootDir\CouchDude.nuspec" -fileTemplates ("$srcDir\bin\$config\CouchDude.*")
    TeamCity-PublishArtifact "$buildDir\CouchDude.$version.nupkg"
}