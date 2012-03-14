param([string]$targetPath)

if (test-path postBuild.user.ps1) {
    . .\postBuild.user.ps1 -targetPath $targetPath
}