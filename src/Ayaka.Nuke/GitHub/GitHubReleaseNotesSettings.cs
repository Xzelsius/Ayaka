// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.Nuke.GitHub;

/// <summary>
///     Provides the settings for generating release notes for a GitHub release.
/// </summary>
[Serializable]
public class GitHubReleaseNotesSettings : GitHubSettings
{
    /// <summary>
    ///     Gets the tag name for the release.
    /// </summary>
    /// <remarks>
    ///     This can be an existing tag or a new one.
    /// </remarks>
    public string Tag => Get<string>(() => Tag);

    /// <summary>
    ///     Gets the commitish value that will be the target for the release's tag.
    /// </summary>
    /// <remarks>
    ///     Required if the supplied <see cref="Tag" /> does not reference an existing tag. Ignored if the <see cref="Tag" />
    ///     already exists.
    /// </remarks>
    public string? TargetCommitish => Get<string?>(() => TargetCommitish);

    /// <summary>
    ///     Gets the name of the previous tag to use as the starting point for the release notes.
    /// </summary>
    /// <remarks>
    ///     Use to manually specify the range for the set of changes considered as part this release.
    /// </remarks>
    public string? PreviousTag => Get<string?>(() => PreviousTag);

    /// <summary>
    ///     Gets the optional path to a file in the repository containing configuration settings used for generating the
    ///     release notes.
    /// </summary>
    /// <remarks>
    ///     If unspecified, the configuration file located in the repository at <c>.github/release.yml</c> or
    ///     <c>.github/release.yaml</c> will be used. If that is not present, the default configuration will be used.
    /// </remarks>
    public string? ConfigFile => Get<string?>(() => ConfigFile);
}
