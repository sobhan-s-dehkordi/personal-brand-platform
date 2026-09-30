#requires -Version 7.0
[CmdletBinding()]
param([string]$Remote = 'origin')
$ErrorActionPreference = 'Stop'
Push-Location (Split-Path $PSScriptRoot -Parent)
try {
    if ((& git config --local --get core.hooksPath) -ne '.githooks') { throw 'Run tools/Initialize-Repository.ps1 before publishing.' }
    $changes = & git status --porcelain
    if ($changes) { throw 'Review and commit your source changes before publishing. This command does not stage files.' }
    & $PSScriptRoot/Test-Publication.ps1 -History
    $branch = & git branch --show-current
    if (!$branch) { throw 'Create a branch before publishing.' }
    & git push --set-upstream $Remote $branch
    if ($LASTEXITCODE -ne 0) { throw 'Push failed. Do not bypass a safety check.' }
} finally { Pop-Location }
