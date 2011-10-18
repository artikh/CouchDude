function exec
{
    [CmdletBinding()]
    param(
        [Parameter(Position=0,Mandatory=1)][scriptblock]$cmd,
        [Parameter(Position=1,Mandatory=0)][string]$errorMessage = ($msgs.error_bad_command -f $cmd)
    )
    & $cmd
    if ($lastexitcode -ne 0) {
        throw ("Exec: " + $errorMessage)
    }
}

if ( (get-command invoke-psake -ErrorAction SilentlyContinue) -eq $null ) {    
    Import-Module .\psake.psm1
}

$globalAssemblyInfo = (cat .\Src\GlobalAssemblyInfo.cs)

$match = [regex]::Match($globalAssemblyInfo, '\[assembly: AssemblyVersion\("([\.\d]+)"\)\]')
$currentVersion = $match.Groups[1].Value

echo "Current version is $currentVersion"

$versionSegments = $currentVersion.Split('.')
$lastVersionSegmentIndex = $versionSegments.Length - 1
$lastVersionSegment = $versionSegments[$lastVersionSegmentIndex]
$lastVersion = [int]::Parse($lastVersionSegment)
$lastVersion++

$versionSegments[$lastVersionSegmentIndex] = $lastVersion.ToString()
$suggestedNewVersion = [string]::Join('.', $versionSegments)

try {
    $newVersion = (Read-Host -Prompt "Enter new version [$suggestedNewVersion]:")
} catch { }
if($newVersion.Length -lt 5) {
    Write-Host "Using suggested version"
    $newVersion = $suggestedNewVersion
}

Write-Host "New version is: $newVersion"
Write-Host "Building..."

$currentDir = (pwd)
try {
    cd .\Scripts; 
    Invoke-Psake .\build.ps1 -framework 4.0x64 -parameters @{ version=$newVersion; config='Release' }; 
} finally {
    cd $currentDir
}

if ($psake.build_success) {
    Write-Host "Committing changes"
    exec { git add -u }
    exec { git commit -m "v$newVersion" }

    Write-Host "Creating tag"
    exec { git tag -f "v$newVersion" }
}

Write-Host "Press any key to continue ..."
$x = $host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")