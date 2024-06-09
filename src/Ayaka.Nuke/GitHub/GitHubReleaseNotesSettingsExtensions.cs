// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.Nuke.GitHub;

using global::Nuke.Common.Tooling;

/// <summary>
///     Extension methods for <see cref="GitHubReleaseNotesSettings" />.
/// </summary>
public static class GitHubReleaseNotesSettingsExtensions
{
    /// <summary>
    ///     Sets the tag name for the release.
    /// </summary>
    /// <param name="settings">The settings instance to adjust.</param>
    /// <param name="tagName">The tag name for the release.</param>
    public static T SetTag<T>(this T settings, string tagName)
        where T : GitHubReleaseNotesSettings
    {
        settings = settings.NewInstance();
        settings.Tag = tagName;

        return settings;
    }

    /// <summary>
    ///     Resets the tag name for the release to <c>null</c>.
    /// </summary>
    /// <param name="settings">The settings instance to adjust.</param>
    public static T ResetTag<T>(this T settings)
        where T : GitHubReleaseNotesSettings
    {
        settings = settings.NewInstance();
        settings.Tag = null;

        return settings;
    }

    /// <summary>
    ///     Sets the commitish value that will be the target for the release's tag.
    /// </summary>
    /// <param name="settings">The settings instance to adjust.</param>
    /// <param name="targetCommitish">The commitish value that will be the target for the release's tag.</param>
    public static T SetTargetCommitish<T>(this T settings, string targetCommitish)
        where T : GitHubReleaseNotesSettings
    {
        settings = settings.NewInstance();
        settings.TargetCommitish = targetCommitish;

        return settings;
    }

    /// <summary>
    ///     Resets the commitish value for the release's tag to <c>null</c>.
    /// </summary>
    /// <param name="settings">The settings instance to adjust.</param>
    public static T ResetTargetCommitish<T>(this T settings)
        where T : GitHubReleaseNotesSettings
    {
        settings = settings.NewInstance();
        settings.TargetCommitish = null;

        return settings;
    }

    /// <summary>
    ///     Sets the name of the previous tag to use as the starting point for the release notes.
    /// </summary>
    /// <param name="settings">The settings instance to adjust.</param>
    /// <param name="previousTag">The name of the previous tag to use as the starting point for the release notes.</param>
    public static T SetPreviousTag<T>(this T settings, string previousTag)
        where T : GitHubReleaseNotesSettings
    {
        settings = settings.NewInstance();
        settings.PreviousTag = previousTag;

        return settings;
    }

    /// <summary>
    ///     Resets the name of the previous tag to use to <c>null</c>.
    /// </summary>
    /// <param name="settings">The settings instance to adjust.</param>
    public static T ResetPreviousTag<T>(this T settings)
        where T : GitHubReleaseNotesSettings
    {
        settings = settings.NewInstance();
        settings.PreviousTag = null;

        return settings;
    }

    /// <summary>
    ///     Sets the path to a file in the repository containing configuration settings used for generating the
    ///     release notes.
    /// </summary>
    /// <param name="settings">The settings instance to adjust.</param>
    /// <param name="configFile">
    ///     The optional path to a file in the repository containing configuration settings used for generating the
    ///     release notes.
    /// </param>
    public static T SetConfigFile<T>(this T settings, string configFile)
        where T : GitHubReleaseNotesSettings
    {
        settings = settings.NewInstance();
        settings.ConfigFile = configFile;

        return settings;
    }

    /// <summary>
    ///     Resets the path to a file in the repository containing configuration settings used for generating the release notes
    ///     to <c>null</c>.
    /// </summary>
    /// <param name="settings">The settings instance to adjust.</param>
    public static T ResetConfigFile<T>(this T settings)
        where T : GitHubReleaseNotesSettings
    {
        settings = settings.NewInstance();
        settings.ConfigFile = null;

        return settings;
    }
}
