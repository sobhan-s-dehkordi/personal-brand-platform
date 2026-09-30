# Contributing

1. Clone the repository and run `pwsh -File tools/Initialize-Repository.ps1`.
2. Create a branch, for example `git switch -c improve-course-editor`.
3. Keep configuration generic and use only synthetic test data.
4. Build and run the relevant tests. SQL tests require a disposable SQL Server environment.
5. Review `git diff` and the exact files staged for commit.
6. Commit normally; the pre-commit hook must pass.
7. Run `pwsh -File tools/Publish.ps1` and open a pull request.

Do not use `--no-verify`, override `core.hooksPath`, or add scanner suppressions to get a failing publication through. Resolve the cause. New file types or binary assets require an explicit review of `.publication-policy.json`; source-code extensions alone do not make a file safe to publish.

Please describe the user-visible behavior, implementation tradeoffs, and validation. Changes to the bilingual editor must preserve two separate records, atomic saves, per-language visibility, and independent workshop capacity and reservations.

Security reports belong in private vulnerability reporting, not public issues. See [SECURITY.md](SECURITY.md).
