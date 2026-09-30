#requires -Version 7.0
[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$version = '8.30.1'
$platform = if ($IsWindows) { 'windows_x64.zip' } elseif ($IsLinux) { 'linux_x64.tar.gz' } else { throw 'Install Gitleaks 8.30.1 manually on this platform.' }
if ([System.Runtime.InteropServices.RuntimeInformation]::OSArchitecture -ne 'X64') { throw 'This installer supports x64 only.' }
$expected = if ($IsWindows) { 'd29144deff3a68aa93ced33dddf84b7fdc26070add4aa0f4513094c8332afc4e' } else { '551f6fc83ea457d62a0d98237cbad105af8d557003051f41f3e7ca7b3f2470eb' }
$destination = Join-Path $root '.local-tools/gitleaks'
New-Item -ItemType Directory -Force $destination | Out-Null
$archive = Join-Path $destination "gitleaks_$version`_$platform"
Invoke-WebRequest "https://github.com/gitleaks/gitleaks/releases/download/v$version/gitleaks_$version`_$platform" -OutFile $archive
if ((Get-FileHash -LiteralPath $archive -Algorithm SHA256).Hash.ToLowerInvariant() -ne $expected) { throw 'Gitleaks archive checksum mismatch. Nothing was installed.' }
if ($IsWindows) { Expand-Archive -LiteralPath $archive -DestinationPath $destination -Force }
else { & tar -xzf $archive -C $destination; if ($LASTEXITCODE -ne 0) { throw 'Gitleaks extraction failed.' } }
Write-Host "Installed checksum-verified Gitleaks $version in the excluded .local-tools directory."
