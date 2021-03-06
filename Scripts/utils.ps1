function verifyVersion([string]$version) {
    $match = [regex]::Match($version, "^(?<mmp>([1-9]\d*|0)\.([1-9]\d*|0)\.([1-9]\d*|0))(\.(?<build>\d+))?$")
    if($match.Success) {
        return $match.Groups['mmp'].Value;
    } else {
        return $null;
    }    
}

function incrementVersion([string]$version, [int]$buildNumber = 1) {
    $versionSegments = $version.Split('.')
    $lastVersionSegmentIndex = $versionSegments.Length - 1
    $lastVersionSegment = $versionSegments[$lastVersionSegmentIndex]
    $lastVersion = [int]::Parse($lastVersionSegment)
    $lastVersion += $buildNumber

    $versionSegments[$lastVersionSegmentIndex] = $lastVersion.ToString()
    return [string]::Join('.', $versionSegments)
}

function readVersion([string]$assemblyInfoFileName) {
    if(-not [string]::IsNullOrEmpty($assemblyInfoFileName) -and (test-path $assemblyInfoFileName)) {
        $assemblyInfo = (cat $assemblyInfoFileName)

        $match = [regex]::Match($assemblyInfo, '\[assembly: AssemblyVersion\("(?<version>[\d\.]+)"\)\]')
        if($match.Success) {      
            return verifyVersion $match.Groups['version'].Value
        }
    }    
    return $null
}

function detectVersion([string]$definedVersion, [string]$assemblyInfoFileName, [int]$buildNumber = 1) {    
    $version = verifyVersion $definedVersion
        
    if($version -eq $null) {
        $readVersion = readVersion $assemblyInfoFileName
        if($readVersion -ne $null) {
            $version = incrementVersion $readVersion $buildNumber
        }
    }
    
    if($version -eq $null) {
        throw "Valid version have not been passed in nor found in $assemblyInfoFileName"
    }
    
    return $version
}

function replaceToken([string]$fileName, [string]$tokenName, [string]$tokenValue) {
    $content = (cat $fileName)
    $content | % { $_.Replace($tokenName, $tokenValue) } > $fileName
}

$nuSpecNamespace = 'http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd'

function prepareNuspec([string]$templateFileName, [string]$targetFileName, [array]$fileTemplates, [string]$packagesFileName) {
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

    $dependencies = $specXml.package.metadata.dependencies;
    $dependencies.RemoveAll();

    if($packagesFileName -ne $null) {
        $packages = (([xml](cat $packagesFileName)).packages.package | %{
            $id = $_.GetAttribute('id')
            $version =  $_.GetAttribute('version')

            $dependencyElement = $specXml.CreateElement("dependency")
            $dependencyElement.SetAttribute('id', $id)
            $dependencyElement.SetAttribute('version', $version)
            $dependencies.AppendChild($dependencyElement)
        })
    }

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

function prepareAndPackage([string]$templateNuSpec, [array]$fileTemplates, [string]$version, [string]$packagesFileName) {
    $nuspecName = [System.IO.Path]::GetFileNameWithoutExtension($templateNuSpec)
    $nuspecFile = "$buildDir\$nuspecName.nuspec"

    prepareNuspec -template "$templateNuspec" -target $nuspecFile -fileTemplates $fileTemplates -packagesFileName $packagesFileName > $null
    replaceToken -file $nuspecFile -tokenName '$version$' -tokenValue $version

    packNuGet $nuspecFile

    del $nuspecFile
}