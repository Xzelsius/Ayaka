// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.Nuke.GitHub;

using global::Nuke.Common;
using global::Nuke.Common.Tooling;
using global::Nuke.Common.Tools.GitHub;

/// <summary>
///     Provides a target for creating a GitHub release to the <see cref="INukeBuild" />.
/// </summary>
public interface ICanGitHubRelease
    : ICan,
        IHaveGitRepository,
        IHaveGitVersion,
        IHaveGitHubToken,
        IHaveGitHubReleaseTarget
{
    /// <summary>
    ///     Gets the base settings for creating release notes.
    /// </summary>
    sealed Configure<GitHubReleaseNotesSettings> GitHubReleaseNotesSettingsBase
        => notes => notes
            .SetRepositoryOwner(GitRepository.GetGitHubOwner())
            .SetRepositoryName(GitRepository.GetGitHubName())
            .SetToken(GitHubToken!)
            .SetTag($"v{Versioning.MajorMinorPatch}")
            .SetTargetCommitish(Versioning.Sha);

    /// <summary>
    ///     Gets the additional settings for creating release notes.
    /// </summary>
    /// <remarks>
    ///     Override this to provide additional settings for the <see cref="IHaveGitHubReleaseTarget.GitHubRelease" /> target.
    /// </remarks>
    Configure<GitHubReleaseNotesSettings> GitHubReleaseNotesSettings
        => notes => notes;

    /// <summary>
    ///     Gets the base settings for creating a GitHub release.
    /// </summary>
    sealed Configure<GitHubReleaseSettings> GitHubReleaseSettingsBase
        => release => release
            .SetRepositoryOwner(GitRepository.GetGitHubOwner())
            .SetRepositoryName(GitRepository.GetGitHubName())
            .SetToken(GitHubToken!)
            .SetTag($"v{Versioning.MajorMinorPatch}")
            .SetTargetCommitish(Versioning.Sha)
            .SetName($"v{Versioning.MajorMinorPatch}");

    /// <summary>
    ///     Gets the additional settings for creating a GitHub release.
    /// </summary>
    /// <remarks>
    ///     Override this to provide additional settings for the <see cref="IHaveGitHubReleaseTarget.GitHubRelease" /> target.
    /// </remarks>
    Configure<GitHubReleaseSettings> GitHubReleaseSettings
        => release => release;

    /// <inheritdoc />
    Target IHaveGitHubReleaseTarget.GitHubRelease => target => target
        .Description("")
        .Unlisted()
        .Requires(() => GitHubToken)
        .Executes(async () =>
        {
            var (_, body) = await GitHubTasks.GenerateReleaseNotes(
                notes => notes
                    .Apply(GitHubReleaseNotesSettingsBase)
                    .Apply(GitHubReleaseNotesSettings));

            await GitHubTasks.CreateRelease(
                release => release
                    .Apply(GitHubReleaseSettingsBase)
                    .SetBody(body)
                    .Apply(GitHubReleaseSettings));
        });
}
