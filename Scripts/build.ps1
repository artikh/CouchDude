Include .\utils.ps1

properties {
    Import-Module .\teamcity.psm1
    
    if($config -eq $null) {
        $config = 'Debug'
    }
    
    $scriptDir = (resolve-path .).Path
    $rootDir = (resolve-path ..).Path;
    $buildDir = "$rootDir\Build";   
    $srcDir = "$rootDir\Src";
    $assemblyInfoFileName = "$rootDir\GlobalAssemblyInfo.cs"
        
    $version = detectVersion $version $assemblyInfoFileName $buildNumber
    $dotNetVersion = formatDotNetVersion $version
    $nuGetVersion = formatNuGetVersion $version
    
    echo "Building version $nuGetVersion"
    
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
    $assembyInfo = $assembyInfo -replace "Version\((.*)\)]", "Version(`"$dotNetVersion`")]"
    $assembyInfo.Trim() > $assemblyInfoFileName
}


task installPackages {    
    dir -Path $rootDir -Recurse -Filter packages.config | %{    
        exec { ..\tools\nuget\NuGet.exe install $_.FullName -Source $nugetSources -OutputDirectory "$rootDir\Packages"  }
    }
}

task build -depends clean, installPackages {
    exec { msbuild "$rootDir\CouchDude.sln" /nologo /p:Configuration=$config /p:Platform='Any Cpu' /maxcpucount }    
}


task test -depends build {
    $outputFileName = "$buildDir\test-results-$dotNetVersion.html"
    exec { & "$rootDir\Tools\xunit\xunit.console.clr4.x86.exe" "$rootDir\Tests\bin\$config\CouchDude.Tests.dll" /silent /-trait level=integration /html "$outputFileName" }
    TeamCity-PublishArtifact $outputFileName
}

task package -depends build {
    prepareAndPackage -templateNuSpec "$rootDir\CouchDude.nuspec" -fileTemplates ("$srcDir\bin\$config\CouchDude.*") -version $nuGetVersion
    TeamCity-PublishArtifact "$buildDir\CouchDude.$nuGetVersion.nupkg"
}