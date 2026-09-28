---
name: renovate-sweep
description: |
  Work through all open Renovate PRs until the list is empty - classify them, approve and auto-merge the safe ones
  one at a time, review majors against their changelogs, fix red builds within a strict non-breaking budget, and
  report everything that needs a decision
argument-hint: "[pr-number ...] [--dry-run]"
disable-model-invocation: true
allowed-tools:
  - Bash(bash .claude/skills/renovate-sweep/scripts/renovate-prs.sh *)
  - Bash(bash .claude/skills/renovate-sweep/scripts/ci-errors.sh *)
  - Bash(bash .claude/skills/renovate-sweep/scripts/wait-rebase.sh *)
  - Bash(bash .claude/skills/renovate-sweep/scripts/watch-pr.sh *)
  - Bash(bash .claude/skills/renovate-sweep/scripts/worktree.sh *)
---

Sweep the open Renovate PRs: $ARGUMENTS. PR numbers restrict the sweep to those PRs. `--dry-run` plans, analyzes
and reports without any GitHub write or push.

Run it from the main checkout. That checkout is never switched: all work happens in ephemeral worktrees
(`scripts/worktree.sh`, root `$RENOVATE_WT_ROOT`, default `${TMPDIR:-/tmp}/ayaka-renovate`). Read `AGENTS.md`
first. The scripts are in `.claude/skills/renovate-sweep/scripts/`; run them with `bash`.

## Rules

- **Writes:** this session does every GitHub write, one at a time. Workers (`renovate-fix`) are read-only.
- **Approvals:** never approve, or enable auto-merge on, a PR that has any non-Renovate commit. If the commit is
  yours, approving would be self-approval, and auto mode blocks that for good reason. The user approves those PRs.
- **Renovate's commit:** never rebase it or force-push over it. Wait for Renovate's own rebase (`wait-rebase.sh`),
  then push on top as a guarded fast-forward (`push-guarded.sh`). A Renovate job already in flight once overwrote
  a fix.
- **Consumer-facing majors:** a PR with `consumerFacing` set is a runtime dependency of a shipped package, or the
  .NET SDK (e.g. .NET, NUKE, `Light.GuardClauses`). Its major can force a major Ayaka release, so never merge or
  fix it. Research it and ask the user.
- **Fixes:** stay within `renovate-fix`'s guardrails and its 20-line budget. Anything more is the user's decision.
- **No PR comments.** The analysis goes into the final report, and the user decides what gets posted.
- **Reruns:** only when a failure has no known error pattern (`ci-errors.sh` prints `PATTERN: none`), and only once.

## Why one PR at a time

`main` requires strict up-to-date checks and dismisses approvals on every push, including Renovate's rebases.
Approving several PRs at once fails: the first merge puts the others behind, Renovate rebases them, and those
rebases dismiss their approvals. So exactly one PR is in flight at a time: up to date and green, then approve,
then auto-merge, then merged, then the next one.

## 1. Inventory

Run `bash .claude/skills/renovate-sweep/scripts/renovate-prs.sh [pr ...]`. It returns JSON per PR: `updateType`,
`deps`, `consumerFacing`, `zeroVer`, `ci`, `behindBy`, `modifiedByOthers`, and more. Re-run it at the start of
every round. The plan is always derived from GitHub's current state, so an interrupted sweep simply continues.
The sweep ends when no remaining PR is one this skill may resolve.

## 2. Plan

Group the PRs by dependency (`deps[].name`, case-insensitive). For each PR, the first matching row wins:

| Condition                                                          | Plan                                                                                                                                                                                                         |
|--------------------------------------------------------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `modifiedByOthers`, and `<root>/pr-<n>.pushed` holds the PR's head | a sweep fixed it. Report "awaiting your approval". If it has fallen behind `main`, tick Renovate's rebase checkbox (Renovate rebuilds the branch without the fix) and send it through the fixed queue again. |
| `modifiedByOthers` otherwise                                       | someone else is working on it. Skip it.                                                                                                                                                                      |
| `ci.stability == pending`                                          | minimum release age not reached yet. Report "waiting"; don't wait for it.                                                                                                                                    |
| `consumerFacing` and `updateType == major`                         | research only (`--research-only`), then report "needs your decision"                                                                                                                                         |
| `major`, or a `minor` with `zeroVer`                               | analysis worker: changelog review, plus a fix if CI is red                                                                                                                                                   |
| `ci.ci == fail`                                                    | run `ci-errors.sh`. `PATTERN: none`: rerun the failed jobs once (`gh run rerun <id> --failed`) and re-check. Otherwise: analysis worker.                                                                     |
| `ci.ci == pending`                                                 | wait for it in the queue (`watch-pr.sh`)                                                                                                                                                                     |
| green digest / patch / minor / pin / lockfile                      | merge queue, unless a major PR for the same dependency is still open. The major goes first. If it merges, Renovate closes this PR. If it ends up unresolved, this PR proceeds.                               |

## 3. Fan out

For each PR that needs a worker:

1. Create its worktree: `worktree.sh add <pr> <branch>`.
2. Start one Agent (general-purpose) for it, **at most 5 at a time**, because local builds are CPU-heavy. Don't
   set `isolation`; the worktree already exists.
3. Give each agent this prompt:

   > Invoke the `renovate-fix` skill with the arguments `<pr> <worktree> [--research-only]`, follow it exactly, and
   > return its verdict JSON as your final answer. The main checkout is `<absolute path>`, and the scripts are in
   > `<absolute path>/.claude/skills/renovate-sweep/scripts/`. Never touch the main checkout.

Check every `fix-then-merge` verdict against the guardrails before accepting it: `fix.linesChanged` ≤ 20,
`publicApi` is `none` or `additive`, and no behavior change in `src/`. If any check fails, treat it as `ask-user`.

What happens with each verdict:

- **`merge`:** the PR goes into the merge queue.
- **`fix-then-merge`:** the PR goes into the fixed queue.
- **`resolve-minor-instead`:** the dependency's minor/digest PR goes into the merge queue, and the major is reported.
- **`ask-user`:** the PR is reported.

## 4. Merge queue (PRs you didn't change)

Order: digest, then pin/lockfile, then patch, then minor, then majors with a `merge` verdict. For each PR, one at a
time:

1. **If it's behind `main`:** run `wait-rebase.sh <branch>`. On timeout, tick Renovate's rebase checkbox (in the PR
   body, `- [ ] <!-- rebase-check -->` becomes `- [x] <!-- rebase-check -->`) and wait once more. If it still
   hasn't rebased, leave it for the next round.
2. **Wait for CI:** `watch-pr.sh <pr> <head>` until the checks finish. If they're red, go back to step 2 (Plan)
   for this PR.
3. **Approve and merge:** `gh pr review <pr> --approve`, then `gh pr merge <pr> --squash --auto`. If GitHub
   refuses auto-merge because the PR is already mergeable, run `gh pr merge <pr> --squash`.
4. **Wait for the merge:** `watch-pr.sh <pr> <head>` until it reports `MERGED`. If the head changed (something
   else moved `main`), the approval was dismissed, so start over at step 1.
5. **Clean up:** `worktree.sh remove <pr>` if a worktree exists. Re-run the inventory before the next PR.

## 5. Fixed queue (after the merge queue is empty)

Fixes are pushed only at this point, one at a time, so they don't go stale or lose approvals while the other PRs
merge:

1. **Wait for Renovate's rebase:** `wait-rebase.sh <branch>`. The branch is still Renovate's alone.
2. **Apply the fix:** `worktree.sh reset <pr> <branch>`, then `git -C <wt> apply <root>/pr-<pr>.patch`. If the
   patch no longer applies, run the worker again.
3. **Commit and verify:**
   - Commit in the worktree with the verdict's `commitMessage`: conventional, never `feat` or `!`, no
     Co-Authored-By trailer.
   - Run the full pipeline there: `dotnet tool restore && dotnet nuke`.
   - Record the verified tree: `git -C <wt> rev-parse 'HEAD^{tree}'`.
4. **Push:** `push-guarded.sh <wt> <branch> <tree>`. If it refuses because `main` moved, go back to step 1.
   After a successful push, write the new head SHA to `<root>/pr-<pr>.pushed`.
5. **Hand over to the user:** run `watch-pr.sh <pr> <new head>`. Once the checks are green, tell the user the PR
   is ready for their approval, with a two-line summary of the fix, as a push notification if available. Then
   wait up to 60 minutes for `MERGED`. If it isn't approved in time, stop and report: the next fixed PR can only
   follow once this one is merged.

## 6. Cleanup and report

- **Cleanup:** `worktree.sh prune` removes worktrees of PRs that are no longer open. Leave the main checkout exactly
  as it was.
- **Report:** a table with columns PR, dependency, `from → to`, update type, outcome and reason. The outcomes:
  - merged
  - merged after your approval
  - awaiting your approval
  - needs your decision
  - left open
  - waiting
  - skipped
- **Analysis for every PR left open:** root cause, the changelog items that affect us, and the proposed diff with
  its size. The user decides whether any of it goes into a PR comment.
