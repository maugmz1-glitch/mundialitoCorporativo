# Git workflow – Mundialito

## Branches

- **main** – Production-ready code; only updated via merge from `release` or hotfix.
- **release** – Release preparation; receives merges from `development` and is used for final testing and version tagging.
- **development** – Default working branch; all feature branches are merged here via Pull Requests.

## Workflow

1. **Create a feature from `development`**
   - `git checkout development && git pull`
   - `git checkout -b feature/teams-pagination` (or `fix/standings-sort`, etc.)

2. **Develop with atomic commits**
   - One logical change per commit (e.g. "Add pageSize validation in GetTeamsQuery", "Add Idempotency-Key to POST teams").
   - Messages: present tense, clear scope (e.g. "Add standings order by goal differential").

3. **Open a Pull Request into `development`**
   - Target: `development`.
   - Describe the change and link any issue.
   - Ensure CI (build + tests) passes.

4. **Merge to `development`**
   - Prefer squash or merge commit per team policy.
   - Delete the feature branch after merge.

5. **Release**
   - From `development`: `git checkout -b release/1.0.0` (or merge into existing `release`).
   - Test and fix only release-critical issues on `release`.
   - When ready: merge `release` into `main` and tag (e.g. `v1.0.0`).
   - Merge `release` back into `development` so both stay in sync.

6. **Hotfix (production only)**
   - From `main`: `git checkout -b hotfix/critical-fix`.
   - Fix and test; merge into `main` and tag.
   - Merge `hotfix` into `development` (and optionally into `release`).

## Summary

- **development** → feature branches → PR → **development**.
- **development** → **release** → test → **main** (tag).
- **main** → hotfix branch → **main** (tag), then merge into **development**.

All integration goes through Pull Requests; direct pushes to `main` and `release` are avoided except for merges.
