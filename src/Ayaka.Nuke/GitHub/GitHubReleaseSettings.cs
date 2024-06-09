namespace Ayaka.Nuke.GitHub;

using Light.GuardClauses;

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
    public virtual string? Tag { get; internal set; }

    /// <summary>
    ///     Gets the commitish value that will be the target for the release's tag.
    /// </summary>
    /// <remarks>
    ///     Unused if the supplied <see cref="Tag" /> already exists. Defaults to the repository's default branch.
    /// </remarks>
    public virtual string? TargetCommitish { get; internal set; }

    /// <summary>
    ///     Gets the name of the release.
    /// </summary>
    public virtual string? Name { get; internal set; }

    /// <summary>
    ///     Gets the text describing the contents of the release.
    /// </summary>
    public virtual string? Body { get; internal set; }

    /// <summary>
    ///     Gets a value indicating whether the release is a draft.
    /// </summary>
    /// <remarks>
    ///     Defaults to <c>false</c>.
    /// </remarks>
    public virtual bool? Draft { get; internal set; }

    /// <summary>
    ///     Gets a value indicating whether the release is a pre-release.
    /// </summary>
    /// <remarks>
    ///     Defaults to <c>false</c>.
    /// </remarks>
    public virtual bool? PreRelease { get; internal set; }

    /// <summary>
    ///     Gets a value indicating whether to automatically generate release notes.
    /// </summary>
    /// <remarks>
    ///     If <see cref="Body" /> is specified, the <see cref="Body" /> will be pre-pended to the automatically generated
    ///     notes. Defaults to <c>false</c>.
    /// </remarks>
    public virtual bool? GenerateReleaseNotes { get; internal set; }

    /// <summary>
    ///     Gets the optional artifact paths to upload to the release.
    /// </summary>
    public virtual IReadOnlyList<string> ArtifactPaths => ArtifactPathsInternal.AsReadOnly();

    internal List<string> ArtifactPathsInternal { get; set; } = [];

    /// <inheritdoc />
    protected override void AssertValid()
    {
        base.AssertValid();

        Tag.MustNotBeNullOrEmpty();

        if (GenerateReleaseNotes != true)
        {
            Name.MustNotBeNullOrEmpty();
            Body.MustNotBeNullOrEmpty();
        }
    }
}
