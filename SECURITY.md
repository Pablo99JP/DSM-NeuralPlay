# Security and SCA policy

This repository follows a lightweight SCA (Software Composition Analysis) policy to keep dependencies reasonably up-to-date and to respond to security advisories.

Key points
- Dependabot is enabled with a weekly cadence for NuGet package updates.
- CI produces SCA artifacts (`sca-reports/vulnerable.json`, `sca-reports/outdated.json`) and a short PR comment summarizing counts.
- High/Critical vulnerabilities reported by the SCA step will fail CI on the Windows runner. The team should triage and remediate immediately.
- For transitive packages flagged by SCA as outdated but with no public advisory, we prefer:
  1. Upgrade direct (top-level) package references to a safe/latest stable version.
  2. If an important transitive package is flagged and cannot be upgraded via a top-level update, add an explicit PackageReference pin in the affected project (with a short justification in the PR).

Remediation workflow
1. Run `dotnet list package --vulnerable --include-transitive` and `dotnet list package --outdated --include-transitive` locally or rely on CI artifacts.
2. Upgrade top-level packages first (run build/tests after each change).
3. If the transitive package cannot be upgraded via top-level changes, pin the transitive package in the project file and explain why in the PR.
4. If the upgrade is large or breaking, open a targeted PR with the migration notes and run full CI matrix.

Contact
If you discover a security issue not covered above, open an issue or contact the maintainers directly.
