#requires -Version 7.0
$ErrorActionPreference = 'Stop'
Push-Location (Split-Path $PSScriptRoot -Parent)
try {
    & git rev-parse --show-toplevel | Out-Null
    if ($LASTEXITCODE -ne 0) { throw 'Clone or initialize the repository first.' }
    & $PSScriptRoot/Install-Gitleaks.ps1
    & git config --local core.hooksPath .githooks
    if ($LASTEXITCODE -ne 0) { throw 'Could not enable repository hooks.' }
    Write-Host 'Pre-commit and pre-push checks enabled for this clone. Never bypass them.'
} finally { Pop-Location }
