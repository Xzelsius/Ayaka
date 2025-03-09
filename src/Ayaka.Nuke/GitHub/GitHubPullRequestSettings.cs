// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.Nuke.GitHub;

/// <summary>
///     Provides the settings for creating a GitHub pull request.
/// </summary>
[Serializable]
public class GitHubPullRequestSettings : GitHubSettings
{
    /// <summary>
    ///     Gets the name of the branch where changes are implemented.
    /// </summary>
    public string Head => Get<string>(() => Head);

    /// <summary>
    ///     Gets the name of the branch where change should be pulled into.
    /// </summary>
    public string Base => Get<string>(() => Base);

    /// <summary>
    ///     Gets the title of the pull request.
    /// </summary>
    public string Title => Get<string>(() => Title);

    /// <summary>
    ///     Gets the text describing the contents of the pull request.
    /// </summary>
    public string? Body => Get<string?>(() => Body);

    /// <summary>
    ///     Gets a value indicating whether the pull request is a draft.
    /// </summary>
    /// <remarks>
    ///     Defaults to <c>false</c>.
    /// </remarks>
    public bool? Draft => Get<bool?>(() => Draft);
}
