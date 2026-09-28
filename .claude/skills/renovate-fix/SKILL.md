---
name: renovate-fix
description: |
  Analyze one Renovate PR - CI errors, upstream changelogs, impact on this repo, local reproduction - and prototype
  a minimal, non-breaking fix in an ephemeral worktree. Read-only on GitHub; returns a verdict for renovate-sweep
argument-hint: "<pr-number> [worktree-path] [--research-only]"
allowed-tools:
  - Bash(bash .claude/skills/renovate-sweep/scripts/ci-errors.sh *)
  - Bash(bash .claude/skills/renovate-sweep/scripts/renovate-prs.sh *)
  - Bash(bash .claude/skills/renovate-sweep/scripts/worktree.sh *)
---

Analyze Renovate PR `$0`. Work in the worktree `$1` if one is given. Otherwise create one from the main checkout
with `bash .claude/skills/renovate-sweep/scripts/worktree.sh add $0 <branch>`, which prints the path.
With `--research-only`, stop after step 3 (changelog and impact) and propose no fix.

Conventions are in `AGENTS.md` and `.claude/rules/`, so read `AGENTS.md` first. The scripts are in the main
checkout under `.claude/skills/renovate-sweep/scripts/`. Call them by absolute path when you work inside the
worktree. Worktrees and saved results share one root, `$RENOVATE_WT_ROOT` (default `${TMPDIR:-/tmp}/ayaka-renovate`).

## Contract

- **Read-only on GitHub:** never push, comment, review, label, merge, close, rerun or edit anything.
- **Work only inside the worktree:** never switch, edit or build the main checkout.
- **Upstream source:** verify third-party APIs and behavior against the real upstream source on GitHub (`gh api`),
  not against the local NuGet cache.
- **Iterate fast:** use `dotnet build Ayaka.slnx -c Release` and filtered `dotnet test --filter ...`. No Python.

## Guardrails

A fix must meet every rule below. If it can't, propose no fix and recommend `ask-user`:

- **Non-breaking.** No public API removed, renamed or changed, no `*REMOVED*` lines, `PublicAPI.Shipped.txt`
  untouched, and no commit that would need `!` or `BREAKING CHANGE`.
- **No runtime behavior change** in `src/`, for any package, stable or preview.
- **Never:**
  - change target frameworks
  - suppress or downgrade analyzers or warnings
  - skip, delete or weaken tests
  - pin or ignore the update
  - touch `.github/`, unless the PR updates a GitHub Action and the fix belongs in the workflow that uses it
- **No new dependencies.** Swapping to a sibling package that upstream recommends is fine
  (`xunit.v3` → `xunit.v3.mtp-off`) if the user approves.
- **Budget: at most 20 changed lines in total**, counted with `git diff --numstat` across every file, including
  `PublicAPI.Unshipped.txt`. For anything larger, report the proposed diff and its size; the user decides.
- **Allowed places:**
  - `test/**` and `eng/*.props`
  - `PublicAPI.Unshipped.txt`, but only entries for members that already exist
  - docs text that names the dependency
  - mechanical renames in `src/` that change no behavior

## Steps

1. **CI.** Run `bash <scripts>/ci-errors.sh $0`. Then find when the branch went red: compare its last green and
   first red runs (`gh api "repos/<owner>/<repo>/actions/runs?branch=<branch>"`) with `main`'s commits in between.
   The cause is often a change on `main` rather than the update itself (#295 broke because of the xUnit v3 migration).
2. **Changelog.** Renovate's notes are often empty or partial. For a major, read every release in the range, not
   only the ones Renovate lists. Sources, in this order:
   - upstream GitHub releases and compare commits
   - the project's docs source
   - `AnalyzerReleases.Shipped.md`
   - nuspec dependencies: `https://api.nuget.org/v3-flatcontainer/<id>/<version>/<id>.nuspec`
   - the package layout: `build/` and `buildTransitive/` props, and the Roslyn version the analyzers reference
3. **Impact.** Search this repo for every API, option and package the relevant changes touch. Mark each change
   "affects us" or "doesn't affect us", with file:line evidence.
4. **Reproduce** in the worktree: build first, then filtered tests. Probe edits are temporary; revert them.
5. **Fix** within the guardrails, checking the known fixes below first. Don't commit; leave the change uncommitted
   in the worktree.
6. **Verify.** Build, run the filtered tests, then the full pipeline in the worktree:
   `dotnet tool restore && dotnet nuke`. Add `dotnet nuke Docs` if docs changed.
7. **Save** both results in the root, not inside the worktree:
   - the patch as `pr-$0.patch`, including new files: `git -C <worktree> add -A`, then
     `git -C <worktree> diff --cached --binary > <root>/pr-$0.patch`. Measure the budget with
     `git -C <worktree> diff --cached --numstat`.
   - the verdict as `pr-$0.json`
8. **Return** the verdict as your final answer. When invoked on its own rather than by the sweep, also tell the user
   the worktree path and that `worktree.sh remove $0` deletes it.

## Verdict

```json
{
  "pr": 277,
  "head": "<sha that was analyzed>",
  "dependency": "FastEndpoints",
  "updateType": "major",
  "from": "6.2.0",
  "to": "8.3.0",
  "ci": "red",
  "rootCause": "one to three sentences",
  "changelog": [{ "version": "7.0.1", "change": "...", "affectsUs": true, "evidence": "test/...:377" }],
  "fix": {
    "patchFile": "<root>/pr-277.patch",
    "files": ["test/..."],
    "linesChanged": 4,
    "touchesSrc": false,
    "publicApi": "none",
    "commitMessage": "test: ..."
  },
  "verification": { "build": "pass", "filteredTests": "7/7", "pipeline": "pass" },
  "recommendation": "fix-then-merge",
  "reason": "why this recommendation",
  "notVerified": ["..."]
}
```

- **`fix`:** `null` when no change is needed or none is proposed. `publicApi` is `none` or `additive`.
  `commitMessage` uses the closed set of types from `AGENTS.md`, never `feat` or `!`.
- **`recommendation`:** one of
  - `merge`: no fix needed; green and the changelog doesn't affect us
  - `fix-then-merge`: a fix within the guardrails and budget, fully verified
  - `resolve-minor-instead`: the major needs more than the guardrails allow, but a minor/digest PR for the same
    dependency can be merged instead
  - `ask-user`: everything else, e.g. over budget, breaking, or unclear. Include the proposed diff if there is one.

## Known fixes

| Symptom                                                                                                                                      | Fix                                                                                                                                                                  |
|----------------------------------------------------------------------------------------------------------------------------------------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `RS0016` for members that already exist (e.g. PublicApiAnalyzers 5 now tracks compiler-generated record members)                             | Copy the exact symbol text from the errors into `PublicAPI.Unshipped.txt`, sorted like `ShipPublicApis` sorts (case-insensitive ordinal, then ordinal), keeping CRLF |
| `CS0400` in `SelfRegisteredExtensions.cs`, `CS1705` for `Microsoft.Testing.Platform`, or "Testing with VSTest target is no longer supported" | The tests run on VSTest (see `ICanDotNetTest`), so they need `xunit.v3.mtp-off`. Never switch to MTP here; that migration is #299.                                   |
| An API moved or was renamed in a test-only dependency (e.g. FastEndpoints 7: `SendStringAsync` → `Send.StringAsync`)                         | Mechanical rename in `test/`, checked against the upstream source of both versions                                                                                   |
| `Ayaka.Nuke` still passes an option the dependency dropped (e.g. `report-warnings`)                                                          | Remove it only if it's proven to be a no-op in every supported version, and update `docs/en/guide/packages/nuke/`                                                    |
