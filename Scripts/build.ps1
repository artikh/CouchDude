properties {
    if ($version -eq $null) {
        throw "Version should be declared explictly"
    }    

    $config = 'Debug'
    $isDebug = $conifg -eq 'Debug'
    
    $scriptDir = (resolve-path .).Path
    $rootDir = (resolve-path ..).Path;
    $buildDir = "$rootDir\Build";    
}

task default -depends build, test, package

task clean {
    if(test-path $buildDir) {
        dir $buildDir | del -Recurse -Force
    }
    mkdir -Force $buildDir
}

task setVersion {
  $assemblyInfoFileName = "$rootDir\Src\GlobalAssemblyInfo.cs"
  
  $assembyInfo = [System.IO.File]::ReadAllText($assemblyInfoFileName)
  $assembyInfo = $assembyInfo -replace "Version\((.*)\)]", "Version(`"$version`")]"
  
  $assembyInfo.Trim() > $assemblyInfoFileName
}

task build -depends clean, setVersion {
    exec { msbuild $rootDir\CouchDude.sln /nologo /p:Config=$config /maxcpucount /verbosity:minimal }
}

task test -depends build {
    exec { ..\Tools\xunit\xunit.console.clr4.x86.exe "$rootDir\Tests\bin\$config\CouchDude.Tests.dll" /silent /-trait "level=integration" /html $buildDir\testResult.html }
}

task package -depends build {
    $packageDir = "$buildDir\package"
    mkdir -Force $packageDir
    copy -Force $rootDir\Src\Core\bin\$config\*.* $packageDir
    mkdir -Force $packageDir\SchemeManager
    mkdir -Force $packageDir\SchemeManager\Console
    copy -Force $rootDir\Src\SchemeManager.Console\bin\$config\*.* $packageDir\SchemeManager\Console
    mkdir -Force $packageDir\SchemeManager\Gui
    copy -Force $rootDir\Src\SchemeManager.Gui\bin\$config\*.* $packageDir\SchemeManager\Gui
    
    exec { ..\Tools\7zip\7za.exe a -mx=9 -bd -r -y $buildDir\couchdude.$version.zip $packageDir\*.* }
}