#requires -Version 7.0
# Synthetic negative tests, run in a temporary repository; no real credentials.
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$temp = Join-Path ([IO.Path]::GetTempPath()) ('personalbrand-guard-test-' + [Guid]::NewGuid().ToString('N'))
$pwsh = (Get-Command pwsh).Source
New-Item -ItemType Directory -Force $temp, "$temp/tools", "$temp/.local-tools/gitleaks" | Out-Null
try {
    Copy-Item "$root/.publication-policy.json", "$root/.gitleaks.toml" $temp
    Copy-Item "$root/tools/Test-Publication.ps1" "$temp/tools/"
    $scannerName = if ($IsWindows) { 'gitleaks.exe' } else { 'gitleaks' }
    Copy-Item "$root/.local-tools/gitleaks/$scannerName" "$temp/.local-tools/gitleaks/"
    if (!$IsWindows) { & chmod +x "$temp/.local-tools/gitleaks/$scannerName" }
    Push-Location $temp
    & git init --quiet -b main
    & git config user.name 'Publication Guard Test'
    & git config user.email 'guard@example.invalid'
    & git config core.hooksPath .disabled-test-hooks
    'Synthetic source fixture.' | Set-Content README.md
    & git add README.md .publication-policy.json .gitleaks.toml tools/Test-Publication.ps1
    & $pwsh -NoProfile -File tools/Test-Publication.ps1 *> $null
    if ($LASTEXITCODE -ne 0) { throw 'Safe baseline was incorrectly rejected.' }
    & git commit --quiet -m 'Safe test baseline'
    function Expect-Rejected([string]$description, [switch]$History) {
        $arguments = @('-NoProfile','-File','tools/Test-Publication.ps1')
        if ($History) { $arguments += '-History' }
        & $pwsh @arguments *> $null
        if ($LASTEXITCODE -eq 0) { throw "Unsafe fixture was accepted: $description" }
        Write-Host "Blocked as expected: $description"
    }
    New-Item -ItemType Directory -Force App_Data | Out-Null
    'Not a real receipt.' | Set-Content App_Data/receipt.txt
    & git add -f App_Data/receipt.txt
    Expect-Rejected 'force-added runtime data'
    & git reset --quiet HEAD -- App_Data/receipt.txt
    New-Item -ItemType Directory -Force src/PersonalBrand.Admin | Out-Null
    '{"Email":{"Password":"synthetic-short-value"}}' | Set-Content src/PersonalBrand.Admin/appsettings.json
    & git add src/PersonalBrand.Admin/appsettings.json
    Expect-Rejected 'credential in tracked configuration'
    & git reset --quiet HEAD -- src/PersonalBrand.Admin/appsettings.json
    # Construct a nonfunctional token-shaped fixture without embedding a token in this script.
    $fake = ('gh' + 'p_') + ('A1b2C3d4' * 4) + 'E5f6'
    ('api_token = "' + $fake + '"') | Set-Content README.md
    & git add README.md
    Expect-Rejected 'recognized token-shaped source text'
    & git reset --quiet HEAD -- README.md
    & git checkout -- README.md
    # A forbidden file in an earlier commit must remain blocked after deletion.
    & git add -f App_Data/receipt.txt
    & git commit --quiet -m 'Synthetic unsafe history'
    & git rm --quiet App_Data/receipt.txt
    & git commit --quiet -m 'Synthetic deletion does not erase history'
    Expect-Rejected 'deleted runtime file in commit history' -History
    Write-Host 'Publication guard regression checks passed.'
} finally {
    Pop-Location
    if ((Test-Path -LiteralPath $temp) -and [IO.Path]::GetFullPath($temp).StartsWith([IO.Path]::GetFullPath([IO.Path]::GetTempPath()), [StringComparison]::OrdinalIgnoreCase) -and (Split-Path $temp -Leaf).StartsWith('personalbrand-guard-test-')) {
        Remove-Item -LiteralPath $temp -Recurse -Force
    }
}
