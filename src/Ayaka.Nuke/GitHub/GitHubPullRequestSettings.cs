// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.Nuke.GitHub;

using Light.GuardClauses;

/// <summary>
///     Provides the settings for creating a GitHub pull request.
/// </summary>
[Serializable]
public class GitHubPullRequestSettings : GitHubSettings
{
    /// <summary>
    ///     Gets the name of the branch where changes are implemented.
    /// </summary>
    public virtual string? Head { get; internal set; }

    /// <summary>
    ///     Gets the name of the branch where change should be pulled into.
    /// </summary>
    public virtual string? Base { get; internal set; }

    /// <summary>
    ///     Gets the title of the pull request.
    /// </summary>
    public virtual string? Title { get; internal set; }

    /// <summary>
    ///     Gets the text describing the contents of the pull request.
    /// </summary>
    public virtual string? Body { get; internal set; }

    /// <summary>
    ///     Gets a value indicating whether the pull request is a draft.
    /// </summary>
    /// <remarks>
    ///     Defaults to <c>false</c>.
    /// </remarks>
    public virtual bool? Draft { get; internal set; }

    /// <inheritdoc />
    protected override void AssertValid()
    {
        base.AssertValid();

        Head.MustNotBeNullOrEmpty();
        Base.MustNotBeNullOrEmpty();
        Title.MustNotBeNullOrEmpty();
    }
}
