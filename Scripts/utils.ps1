function parseVersion([string]$versionString) {
    
    $match = [regex]::Match($versionString, '^(?<major>\d+)\.(?<minor>\d+)\.(?<patch>\d+)(\.(?<build>\d+))?$')
    
    if(-not $match.Success) {
        return $null;
    }
    $major = $match.Groups['major'].Value
    $minor = $match.Groups['minor'].Value
    $patch = $match.Groups['patch'].Value
    
    $buildGroup = $match.Groups['build']
    if($buildGroup.Success) {
        $build = $buildGroup.Value
    } else {
        $build = '0'
    }
        
    return ($major, $minor, $patch, $build)
}

function detectVersion([string]$versionString, [string]$assemblyInfoFileName, [string]$buildNumber) {
    [array]$version = $null;
    
    if( -not [string]::IsNullOrEmpty($versionString)) {
        $version = parseVersion $versionString
    }
    
    if($version -eq $null -and -not [string]::IsNullOrEmpty($assemblyInfoFileName) -and (test-path $assemblyInfoFileName)) {
        $assemblyInfo = (cat $assemblyInfoFileName)

        $match = [regex]::Match($assemblyInfo, '\[assembly: AssemblyVersion\("(?<version>[\d\.]+)"\)\]')
        if($match.Success) {
            $version = parseVersion ($match.Groups['version'].Value)
        }
    }
    
    if($version -eq $null) {
        $version = '0','0','0','0'
    }
    
    if(-not [string]::IsNullOrEmpty($buildNumber)) {
        $version[3] = $buildNumber
    }
    
    return $version
}

function formatNuGetVersion([array]$version) {
    if($version -eq $null) { return $null; }
    return $version[0] + '.' + $version[1] + '.' + $version[2] + '-build' + $version[3]
}

function formatDotNetVersion([array]$version) {
    if($version -eq $null) { return $null; }
    return $version[0] + '.' + $version[1] + '.' + $version[2] + '.' + $version[3]
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

function prepareAndPackage([string]$templateNuSpec, [array]$fileTemplates, [string]$version) {
    $nuspecName = [System.IO.Path]::GetFileNameWithoutExtension($templateNuSpec)
    $nuspecFile = "$buildDir\$nuspecName.nuspec"

    prepareNuspec -template "$templateNuspec" -target $nuspecFile -fileTemplates $fileTemplates > $null
    replaceToken -file $nuspecFile -tokenName '$version$' -tokenValue $version

    packNuGet $nuspecFile

    del $nuspecFile
}