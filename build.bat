@ECHO OFF

SET /P COUCHDB_VERSION=Enter package version:

powershell -NoLogo -NoProfile -NonInteractive -ExecutionPolicy Unrestricted -Command "&{ cd .\Scripts; Import-Module .\psake.psm1; Invoke-Psake .\build.ps1 -framework 4.0x64 -parameters @{ version='%COUCHDB_VERSION%' } }; 

PAUSE