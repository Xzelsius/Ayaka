// Copyright (c) Raphael Strotz. All rights reserved.

using System.Linq;
using Ayaka.Nuke;
using Ayaka.Nuke.GitHub;
using Ayaka.Nuke.PublicApi;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.Git;
using Nuke.Common.Utilities.Collections;
using Serilog;
using OfficialGitHubTasks = Nuke.Common.Tools.GitHub.GitHubTasks;

partial class Build
{
    const string AfterReleaseBranchName = "after-release";

    Target CreateAfterReleaseBranch => target => target
        .Unlisted()
        .Before<IHaveShipPublicApisTarget>()
        .Requires(() => GitTasks.GitHasCleanWorkingCopy())
        .Executes(() =>
        {
            Log.Information("Checking for existing remote branch {BranchName}", AfterReleaseBranchName);

            _ = GitTasks.Git("fetch --quiet --no-tags origin");

            var branchExists = GitTasks
                .Git(
                    $"rev-parse --verify --quiet \"remotes/origin/{AfterReleaseBranchName}\"",
                    exitHandler: process => process.AssertWaitForExit())
                .Any();

            if (branchExists)
            {
                Log.Error("Branch '{BranchName}' for after-release changes already exists", AfterReleaseBranchName);
            }

            Assert.False(branchExists);

            Log.Information("Switching to new branch {BranchName}", AfterReleaseBranchName);

            _ = GitTasks.Git($"checkout --quiet --no-track -b {AfterReleaseBranchName} origin/main");
        });

    [Parameter("The Git user email address")]
    string? GitUserEmail;

    [Parameter("The Git user name")]
    string? GitUserName;

    bool skipPullRequest; // will be set to true if no after-release changes were detected

    Target CommitAndPushAfterReleaseChanges => target => target
        .Unlisted()
        .DependsOn(CreateAfterReleaseBranch)
        .DependsOn<IHaveShipPublicApisTarget>()
        .Requires(() => GitUserEmail, () => GitUserName)
        .Executes(() =>
        {
            Log.Information("Configuring the Git user");

            _ = GitTasks.Git($"config user.email {GitUserEmail}");
            _ = GitTasks.Git($"config user.name {GitUserName}");

            Log.Information("Staging pending changes on PublicAPI.*.txt files");

            From<IHaveSources>()
                .SourceDirectory
                .GlobFiles("**/PublicAPI.*.txt")
                .ForEach(file =>
                {
                    _ = GitTasks.Git($"add {file}");
                });

            Log.Information("Checking if there are staged changes");

            var hasChanges = GitTasks.Git("status --porcelain --untracked-files no").Any();
            if (!hasChanges)
            {
                Log.Warning("No after release changes detected. Skipping commit & push");
                skipPullRequest = true;
                return;
            }

            const string message = "chore: ship public apis";

            Log.Information("Commiting staged changes on PublicAPI.*.txt files");

            _ = GitTasks.Git($"commit --quiet -m {message}");

            Log.Information("Pushing changes to remote");

            _ = GitTasks.Git(
                $"push --quiet -u origin {AfterReleaseBranchName}",
                logOutput: false /* GitHub writes some weird stuff back */);

            Log.Information("Done");
        });

    Target CreateAfterReleasePullRequest => target => target
        .Unlisted()
        .DependsOn(CommitAndPushAfterReleaseChanges)
        .Requires(() => From<IHaveGitHubToken>().GitHubToken)
        .OnlyWhenDynamic(() => !skipPullRequest)
        .Executes(async () =>
        {
            await GitHubTasks.CreatePullRequest(
                pullRequest => pullRequest
                    .SetRepositoryOwner(OfficialGitHubTasks.GetGitHubOwner(From<IHaveGitRepository>().GitRepository))
                    .SetRepositoryName(OfficialGitHubTasks.GetGitHubName(From<IHaveGitRepository>().GitRepository))
                    .SetToken(From<IHaveGitHubToken>().GitHubToken!)
                    .SetHead(AfterReleaseBranchName)
                    .SetBase("main")
                    .SetTitle("chore: after release changes")
                    .SetBody("""
                             **This PR was automatically generated after the release**
                             
                             - Ships the public APIs for the latest release.
                             """));
        });
}
