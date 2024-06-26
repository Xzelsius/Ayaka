// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.Nuke.GitHub;

using global::Nuke.Common.Tooling;

/// <summary>
///     Extension methods for <see cref="GitHubSettings" />.
/// </summary>
public static class GitHubSettingsExtensions
{
    /// <summary>
    ///     Sets the account owner of the repository.
    /// </summary>
    /// <param name="settings">The settings instance to adjust.</param>
    /// <param name="repositoryOwner">The account owner of the repository.</param>
    public static T SetRepositoryOwner<T>(this T settings, string repositoryOwner)
        where T : GitHubSettings
    {
        settings = settings.NewInstance();
        settings.RepositoryOwner = repositoryOwner;

        return settings;
    }

    /// <summary>
    ///     Resets the account owner of the repository to <c>null</c>.
    /// </summary>
    /// <param name="settings">The settings instance to adjust.</param>
    public static T ResetRepositoryOwner<T>(this T settings)
        where T : GitHubSettings
    {
        settings = settings.NewInstance();
        settings.RepositoryOwner = null;

        return settings;
    }

    /// <summary>
    ///     Sets the name of the repository without the .git extension.
    /// </summary>
    /// <param name="settings">The settings instance to adjust.</param>
    /// <param name="repositoryName">The name of the repository without the .git extension.</param>
    public static T SetRepositoryName<T>(this T settings, string repositoryName)
        where T : GitHubSettings
    {
        settings = settings.NewInstance();
        settings.RepositoryName = repositoryName;

        return settings;
    }

    /// <summary>
    ///     Resets the name of the repository to <c>null</c>.
    /// </summary>
    /// <param name="settings">The settings instance to adjust.</param>
    public static T ResetRepositoryName<T>(this T settings)
        where T : GitHubSettings
    {
        settings = settings.NewInstance();
        settings.RepositoryName = null;

        return settings;
    }

    /// <summary>
    ///     Sets the GitHub token used to authenticate with the GitHub API.
    /// </summary>
    /// <param name="settings">The settings instance to adjust.</param>
    /// <param name="token">The GitHub token used to authenticate with the GitHub API.</param>
    public static T SetToken<T>(this T settings, string token)
        where T : GitHubSettings
    {
        settings = settings.NewInstance();
        settings.Token = token;

        return settings;
    }

    /// <summary>
    ///     Resets the GitHub token to <c>null</c>.
    /// </summary>
    /// <param name="settings">The settings instance to adjust.</param>
    public static T ResetToken<T>(this T settings)
        where T : GitHubSettings
    {
        settings = settings.NewInstance();
        settings.Token = null;

        return settings;
    }

    /// <summary>
    ///     Sets the base URL for the GitHub API.
    /// </summary>
    /// <param name="settings">The settings instance to adjust.</param>
    /// <param name="baseUrl">The base URL for the GitHub API.</param>
    public static T SetBaseUrl<T>(this T settings, string baseUrl)
        where T : GitHubSettings
    {
        settings = settings.NewInstance();
        settings.BaseUrl = baseUrl;

        return settings;
    }

    /// <summary>
    ///     Resets the base URL for the GitHub API to <c>null</c>.
    /// </summary>
    /// <param name="settings">The settings instance to adjust.</param>
    public static T ResetBaseUrl<T>(this T settings)
        where T : GitHubSettings
    {
        settings = settings.NewInstance();
        settings.BaseUrl = null;

        return settings;
    }
}