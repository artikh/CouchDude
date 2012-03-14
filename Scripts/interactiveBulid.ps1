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
    Import-Module .\Scripts\psake.psm1
}

$globalAssemblyInfo = (cat .\GlobalAssemblyInfo.cs)

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

while($true) {
    try {
        $newVersion = (Read-Host -Prompt "Enter new version [$suggestedNewVersion]")
    } catch { }

    if ($newVersion.ToLower().StartsWith('c')) {
        Write-Host "Using current version"
        $newVersion = $currentVersion
    } else {
        if ($newVersion.Length -eq 0) {
            $newVersion = $suggestedNewVersion;
        }
        Write-Host "Using version $newVersion"
    }

    if( -not ($newVersion -match "^([1-9]\d*|0)\.([1-9]\d*|0)\.([1-9]\d*|0)$")) {
        Write-Host "Version is malformed please try again"
    }
    else {
        break
    }
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

if($commit -ne $null -and ($commit.ToLower().StartsWith('t') -or $commit.ToLower().StartsWith('y'))) {
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
    exec { git commit }

    Write-Host "Creating tag"
    exec { git tag -f "v$newVersion" }
}


Write-Host "Press any key to continue ..."
$x = $host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
