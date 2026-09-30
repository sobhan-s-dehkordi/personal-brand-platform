# Security policy

## Report a vulnerability privately

Use this repository's **Security → Report a vulnerability** form. Do not post credentials, private reservation URLs, customer information, receipts, or exploit details in public issues. If private reporting is unavailable, do not open a public report containing sensitive data.

Only the current main branch is maintained. Security reports are reviewed on a best-effort basis; this project makes no production security or response-time guarantee.

## Secrets and operational data

Use User Secrets locally and a secret manager or protected environment in production. Keep database files, backups, uploads, receipts, logs, exported contacts, authentication cookies, and data-protection keys outside source control. Test fixtures must contain synthetic data only.

The application implements authorization, antiforgery, input validation, sanitized rich text, private receipt handling, and transactional reservation workflows. These controls do not replace production deployment review, patching, HTTPS configuration, backups, or access management.

## If something is accidentally exposed

Revoke or rotate affected credentials immediately. Removing a file in a later commit does not remove it from Git history, clones, caches, or forks. Coordinate history cleanup and assess any personal-data exposure. Never paste the exposed value into an issue, commit message, or CI log.

See [Safe publishing](docs/SAFE-PUBLISHING.md) for the repository controls and their limitations.
