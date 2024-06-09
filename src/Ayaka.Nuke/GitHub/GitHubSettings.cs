// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.Nuke.GitHub;

using global::Nuke.Common.Tooling;
using Light.GuardClauses;

/// <summary>
///     Provides the base settings for interacting with the GitHub API.
/// </summary>
[Serializable]
public class GitHubSettings : ISettingsEntity
{
    /// <summary>
    ///     Gets the account owner of the repository.
    /// </summary>
    /// <remarks>
    ///     The name is not case sensitive.
    /// </remarks>
    public virtual string? RepositoryOwner { get; internal set; }

    /// <summary>
    ///     Gets the name of the repository without the .git extension.
    /// </summary>
    /// <remarks>
    ///     The name is not case sensitive.
    /// </remarks>
    public virtual string? RepositoryName { get; internal set; }

    /// <summary>
    ///     Gets the GitHub token used to authenticate with the GitHub API.
    /// </summary>
    public virtual string? Token { get; internal set; }

    /// <summary>
    ///     Gets the base URL for the GitHub API.
    /// </summary>
    /// <remarks>
    ///     Mainly used for GitHub enterprise.
    /// </remarks>
    public virtual string? BaseUrl { get; internal set; }

    /// <summary>
    ///     Validates this instance of GitHub settings.
    /// </summary>
    public void Validate() => AssertValid();

    /// <summary>
    ///     Asserts that the settings are valid.
    /// </summary>
    protected virtual void AssertValid()
    {
        RepositoryOwner.MustNotBeNullOrEmpty();
        RepositoryName.MustNotBeNullOrEmpty();
        Token.MustNotBeNullOrEmpty();
    }
}
