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
    $newVersion = (Read-Host -Prompt "Enter new version [$suggestedNewVersion]")
} catch { }

if ($newVersion.StartsWith('c')) {
    Write-Host "Using current version"
    $newVersion = $currentVersion
} else {
    Write-Host "Using version $newVersion"
}

try {
    $taskList = (Read-Host -Prompt "Enter task list [default]")
} catch { }

if($taskList -eq $null -or $taskList -eq '') {
    $taskList = ('default')
}

try {
    $commit = (Read-Host -Prompt "Commit and create tag? [False]")
} catch { }

if($commit -ne $null -and $commit.StartsWith('t')) {
    $commit = $true
} else {
    $commit = $false
}

Write-Host "Building..."

$currentDir = (pwd)
try {
    cd .\Scripts; 
    Invoke-Psake .\build.ps1 -framework 4.0x64 -parameters @{ version=$newVersion; config='Release' } -taskList $taskList; 
} finally {
    cd $currentDir
}

if ($psake.build_success -and $commit) {
    Write-Host "Committing changes"
    exec { git add -u }
    exec { git commit -m "v$newVersion" }

    Write-Host "Creating tag"
    exec { git tag -f "v$newVersion" }
}

Write-Host "Press any key to continue ..."
$x = $host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")