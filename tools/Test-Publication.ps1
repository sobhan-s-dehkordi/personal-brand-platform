#requires -Version 7.0
[CmdletBinding()]
param([switch]$History)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$root = Split-Path $PSScriptRoot -Parent
Push-Location $root
$temp = Join-Path ([IO.Path]::GetTempPath()) ('personalbrand-publication-' + [Guid]::NewGuid().ToString('N'))
try {
    $policy = Get-Content -LiteralPath '.publication-policy.json' -Raw | ConvertFrom-Json -AsHashtable
    $scannerRelative = if ($IsWindows) { '.local-tools/gitleaks/gitleaks.exe' } else { '.local-tools/gitleaks/gitleaks' }
    $scanner = Join-Path $root $scannerRelative
    if (!(Test-Path -LiteralPath $scanner)) { throw 'Publication blocked: run tools/Install-Gitleaks.ps1 first.' }
    $version = & $scanner version
    if ($LASTEXITCODE -ne 0 -or $version.Trim() -ne '8.30.1') { throw 'Publication blocked: expected Gitleaks 8.30.1.' }
    function Assert-Path([string]$path, [string]$mode) {
        if ($mode -notin @('100644','100755')) { throw "Publication blocked: symlinks/submodules are not allowed ($path)." }
        if ($path -match '[\x00-\x1f]' -or $path -match $policy.deniedPaths -or !($policy.allowedPaths | Where-Object { $path -cmatch $_ })) {
            throw "Publication blocked: unapproved file path: $path"
        }
    }
    function Assert-Configuration($value, [string]$path, [string]$key = '') {
        if ($value -is [System.Collections.IDictionary]) {
            foreach ($entry in $value.GetEnumerator()) { Assert-Configuration $entry.Value $path ($key + '.' + $entry.Key) }
        } elseif ($value -is [string] -and $value.Length -gt 0) {
            if ($key -match '(?i)(Password|Token|ApiKey|Secret|PrivateKey|CardNumber)$' -or $key -match '(?i)\.Email\.(Host|From|Username)$' -or $key -match '(?i)\.Telegram\.ChatId$' -or $value -match '(?i)(Password|Pwd)\s*=\s*[^;\s]+') {
                throw "Publication blocked: operational value in $path at $key. Use environment variables or User Secrets."
            }
        }
    }
    function Assert-File([string]$fullPath, [string]$path) {
        $file = Get-Item -LiteralPath $fullPath -Force
        if ($file.Length -gt $policy.maxFileBytes) { throw "Publication blocked: file exceeds 2 MiB ($path)." }
        $bytes = [IO.File]::ReadAllBytes($fullPath)
        $binary = [IO.Path]::GetExtension($path) -in @('.woff2','.ico') -or $bytes.Contains([byte]0)
        if ($binary) {
            $hash = (Get-FileHash -LiteralPath $fullPath -Algorithm SHA256).Hash.ToLowerInvariant()
            if ($policy.approvedBinaryHashes[$path] -ne $hash) { throw "Publication blocked: unreviewed binary file ($path)." }
        } else {
            $text = [Text.UTF8Encoding]::new($false, $true).GetString($bytes)
            if ([IO.Path]::GetFileName($path) -match '^appsettings.*\.json$') { Assert-Configuration ($text | ConvertFrom-Json -AsHashtable) $path }
        }
    }
    New-Item -ItemType Directory $temp | Out-Null
    $index = @(& git -c core.quotepath=false ls-files --stage)
    if ($LASTEXITCODE -ne 0 -or $index.Count -eq 0) { throw 'Publication blocked: no staged repository content to inspect.' }
    foreach ($entry in $index) {
        if ($entry -notmatch '^(\d+) ([a-f0-9]+) 0\t(.+)$') { throw 'Publication blocked: unresolved or unsupported index entry.' }
        Assert-Path $Matches[3] $Matches[1]
    }
    $prefix = $temp.Replace('\','/') + '/'
    & git checkout-index --all "--prefix=$prefix"
    if ($LASTEXITCODE -ne 0) { throw 'Cannot inspect the staged snapshot.' }
    foreach ($file in Get-ChildItem -LiteralPath $temp -File -Recurse -Force) {
        $path = [IO.Path]::GetRelativePath($temp, $file.FullName).Replace('\','/')
        Assert-File $file.FullName $path
    }
    $config = Join-Path $temp '.gitleaks.toml'
    & $scanner dir $temp --config $config --gitleaks-ignore-path $temp --ignore-gitleaks-allow --redact --no-banner --log-level warn
    if ($LASTEXITCODE -ne 0) { throw 'Publication blocked: the staged snapshot failed secret scanning.' }
    if ($History) {
        $commits = @(& git rev-list --all)
        if ($LASTEXITCODE -ne 0 -or $commits.Count -eq 0) { throw 'Publication blocked: no commit history to inspect.' }
        $checked = [Collections.Generic.HashSet[string]]::new()
        foreach ($commit in $commits) {
            foreach ($entry in @(& git -c core.quotepath=false ls-tree -r --full-tree $commit)) {
                if ($entry -notmatch '^(\d+) (?:blob|commit) ([a-f0-9]+)\t(.+)$') { throw 'Unsupported history entry.' }
                if ($checked.Add($entry)) {
                    $path = $Matches[3]; $mode = $Matches[1]; $objectId = $Matches[2]
                    Assert-Path $path $mode
                    # Inspect the historical bytes, not just today's working-tree copy.
                    $historicalFile = Join-Path $temp '.historical-blob'
                    $start = [Diagnostics.ProcessStartInfo]::new('git')
                    foreach ($argument in @('cat-file','blob',$objectId)) { $start.ArgumentList.Add($argument) }
                    $start.RedirectStandardOutput = $true
                    $start.RedirectStandardError = $true
                    $process = [Diagnostics.Process]::Start($start)
                    $stream = [IO.File]::Create($historicalFile)
                    try { $process.StandardOutput.BaseStream.CopyTo($stream) } finally { $stream.Dispose() }
                    $process.WaitForExit()
                    if ($process.ExitCode -ne 0) { throw 'Cannot inspect historical file contents.' }
                    $process.Dispose()
                    Assert-File $historicalFile $path
                }
            }
            if ($LASTEXITCODE -ne 0) { throw 'Cannot inspect commit paths.' }
        }
        & $scanner git $root --log-opts=--all --config $config --gitleaks-ignore-path $temp --ignore-gitleaks-allow --redact --no-banner --log-level warn
        if ($LASTEXITCODE -ne 0) { throw 'Publication blocked: commit history failed secret scanning.' }
    }
    Write-Host "Publication checks passed: $($index.Count) approved staged files; history=$History."
} finally {
    # Only remove the uniquely named directory created by this invocation.
    if ((Test-Path -LiteralPath $temp) -and [IO.Path]::GetFullPath($temp).StartsWith([IO.Path]::GetFullPath([IO.Path]::GetTempPath()), [StringComparison]::OrdinalIgnoreCase) -and (Split-Path $temp -Leaf).StartsWith('personalbrand-publication-')) {
        Remove-Item -LiteralPath $temp -Recurse -Force
    }
    Pop-Location
}

