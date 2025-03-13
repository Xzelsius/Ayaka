// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.Nuke.GitHub;

using global::Nuke.Common.Tooling;

/// <summary>
///     Provides the base settings for interacting with the GitHub API.
/// </summary>
[Serializable]
public class GitHubSettings : Options
{
    /// <summary>
    ///     Gets the account owner of the repository.
    /// </summary>
    /// <remarks>
    ///     The name is not case-sensitive.
    /// </remarks>
    public string RepositoryOwner => Get<string>(() => RepositoryOwner);

    /// <summary>
    ///     Gets the name of the repository without the .git extension.
    /// </summary>
    /// <remarks>
    ///     The name is not case-sensitive.
    /// </remarks>
    public string RepositoryName => Get<string>(() => RepositoryName);

    /// <summary>
    ///     Gets the GitHub token used to authenticate with the GitHub API.
    /// </summary>
    public string Token => Get<string>(() => Token);

    /// <summary>
    ///     Gets the base URL for the GitHub API.
    /// </summary>
    /// <remarks>
    ///     Mainly used for GitHub enterprise.
    /// </remarks>
    public string? BaseUrl => Get<string?>(() => BaseUrl);
}
