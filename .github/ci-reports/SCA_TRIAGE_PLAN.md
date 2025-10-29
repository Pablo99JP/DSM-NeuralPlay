SCA Triage Plan
================

This document contains a short, low-risk plan to triage the `.github/ci-reports/outdated.json`
report and make safe, incremental dependency updates.

Goals
-----
- Prioritize safety: avoid upgrading to packages that target a newer runtime (e.g. .NET 9) if the
  repository targets .NET 8.
- Fix actual vulnerabilities first (High/Critical) found in `.github/ci-reports/vulnerable.json`.
- Treat transitive "outdated" entries as informational; only pin or upgrade when a security
  advisory or CI failure justifies it.

Suggested steps (safe, actionable)
----------------------------------
1. Confirm there are no High/Critical entries in `.github/ci-reports/vulnerable.json`. If any,
   address them immediately (open PRs that update the affected top-level packages or add
   explicit transitive pins with justification).

2. Do NOT mass-upgrade `Microsoft.*` libraries to `9.x` while the codebase targets `net8.0`.
   Those packages are often runtime-targeted; upgrading may lead to subtle breakage. Instead,
   prefer:
   - Upgrading direct dependencies only (those listed in `.csproj` files).
   - Adding explicit transitive `PackageReference` pins only when a vulnerability or test-failure
     demonstrates the need.

3. Keep Dependabot enabled (see `.github/dependabot.yml`) to open informative PRs weekly. Review
   Dependabot PRs and accept those that clearly upgrade direct dependencies and have green CI.

4. Prioritize these direct or high-impact items (examples from `outdated.json`):
   - `Newtonsoft.Json` (minor patch available: 13.0.3 -> 13.0.4) — safe, low-risk patch.
   - `xunit.analyzers` — safe to update tooling/test-analyzers.
   - Any `Microsoft.Identity.Client` minor updates if used directly by a project.

5. For runtime/native `System.*` packages flagged as outdated (many are transitive):
   - Treat them as low priority unless a vulnerability is reported.
   - If a CI runner shows runtime incompatibilities, pin a transitive package in the affected project
     and run full test matrix.

6. Make small, single-purpose PRs. Each PR should:
   - Update 1 package (or closely related packages)
   - Include a short justification in the PR body mentioning the SCA report reference
   - Run CI and ensure all tests pass before merging

7. Document every pinned transitive package in the PR and in `SECURITY.md` (reason and date).

Next actions I can do now (you can approve):
- Create safe PRs for low-risk direct upgrades such as `Newtonsoft.Json` and `xunit.analyzers`.
- Leave transitive runtime upgrades to Dependabot and CI unless a vulnerability requires intervention.

If you want I can create the PRs for the safe direct upgrades now and run tests locally before pushing.

Created by automation on behalf of repository maintainers.
