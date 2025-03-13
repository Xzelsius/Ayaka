// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.Nuke.GitHub;

/// <summary>
///     Provides the settings for creating a GitHub release.
/// </summary>
[Serializable]
public class GitHubReleaseSettings : GitHubSettings
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
    ///     Unused if the supplied <see cref="Tag" /> already exists. Defaults to the repository's default branch.
    /// </remarks>
    public string? TargetCommitish => Get<string?>(() => TargetCommitish);

    /// <summary>
    ///     Gets the name of the release.
    /// </summary>
    public string? Name => Get<string?>(() => Name);

    /// <summary>
    ///     Gets the text describing the contents of the release.
    /// </summary>
    public string? Body => Get<string?>(() => Body);

    /// <summary>
    ///     Gets a value indicating whether the release is a draft.
    /// </summary>
    /// <remarks>
    ///     Defaults to <c>false</c>.
    /// </remarks>
    public bool? Draft => Get<bool?>(() => Draft);

    /// <summary>
    ///     Gets a value indicating whether the release is a pre-release.
    /// </summary>
    /// <remarks>
    ///     Defaults to <c>false</c>.
    /// </remarks>
    public bool? PreRelease => Get<bool?>(() => PreRelease);

    /// <summary>
    ///     Gets a value indicating whether to automatically generate release notes.
    /// </summary>
    /// <remarks>
    ///     If <see cref="Body" /> is specified, the <see cref="Body" /> will be pre-pended to the automatically generated
    ///     notes. Defaults to <c>false</c>.
    /// </remarks>
    public bool? GenerateReleaseNotes => Get<bool?>(() => GenerateReleaseNotes);

    /// <summary>
    ///     Gets the optional artifact paths to upload to the release.
    /// </summary>
    public IReadOnlyList<string>? ArtifactPaths => Get<List<string>?>(() => ArtifactPaths);
}
