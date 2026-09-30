# Safe publishing

## What belongs in the repository

Application source, synthetic tests, reviewed documentation, generic development configuration, and the specific vendored assets required to run the UI. The approved fonts and icons have exact hashes in `.publication-policy.json`.

## What stays local

`App_Data`, SQL databases/backups, uploads, private receipts, contact exports, logs, test reports, local preview HTML, internal delivery screenshots/reports, IDE state, credentials, certificates, data-protection keys, downloaded tools, and build outputs.

The root `.gitignore` uses an allowlist. A separate publication policy checks staged paths and content, so `git add -f` does not silently defeat the pre-commit checks. Operational JSON settings are checked for credential values as well as scanned for recognized secrets. Unexpected binary files are rejected.

## Install the controls for every clone

```powershell
pwsh -File tools/Initialize-Repository.ps1
```

This downloads Gitleaks 8.30.1 from its official release, verifies a pinned SHA-256 checksum, and sets the clone's local `core.hooksPath` to `.githooks`. Windows and Linux x64 are supported by the installer. PowerShell 7 must be on PATH when Git invokes the hooks. If a prerequisite is missing, the hooks stop the operation.

## Everyday workflow

```powershell
git switch -c describe-your-change
# Make changes and run the appropriate tests.
git status --short
git diff
# Stage specific reviewed files, then inspect the staged diff.
git add src/PersonalBrand.Admin/Pages/Content/Edit.cshtml
git diff --cached
git commit -m "Describe the behavior change"
pwsh -File tools/Publish.ps1
```

Open a pull request and wait for the required checks. `Publish.ps1` requires a clean working tree and does not stage files automatically. The pre-push hook scans all locally reachable history, including previous commits where a sensitive file might have been added and later removed. Do not share branches with unsafe commits.

## GitHub controls

The maintained repository enables secret scanning and push protection, vulnerability alerts, and private vulnerability reporting. Its main branch requires passing publication-safety and application-test checks, and disallows force-pushes and deletion. Review security settings after transferring or recreating the repository; these settings are not installed by cloning it.

GitHub push protection recognizes supported secret formats. It is not an arbitrary-data firewall. File-path push rules are not available for this public personal repository, so the local hooks and manual review remain necessary. CI runs after upload and is not a substitute for pre-push checks.

## Limits

Git hooks can be bypassed or omitted on another machine. Repository owners can change protection settings. Scanners cannot reliably recognize every password, private document, or personal detail embedded in ordinary text. These controls reduce risk; they cannot make future publication infallible. Never use a bypass to publish a failing change.

If a credential escapes, revoke it first. Deleting the file from the latest revision is not sufficient. Consult [SECURITY.md](../SECURITY.md).
