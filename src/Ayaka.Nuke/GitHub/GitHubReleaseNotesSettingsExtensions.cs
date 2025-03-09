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
    [Builder(
        Type = typeof(GitHubReleaseNotesSettings),
        Property = nameof(GitHubReleaseNotesSettings.Tag))]
    public static T SetTag<T>(this T settings, string tagName)
        where T : GitHubReleaseNotesSettings
        => settings.Modify(
            x => x.Set(
                () => settings.Tag,
                tagName));

    /// <summary>
    ///     Resets the tag name for the release to <c>null</c>.
    /// </summary>
    /// <param name="settings">The settings instance to adjust.</param>
    [Builder(
        Type = typeof(GitHubReleaseNotesSettings),
        Property = nameof(GitHubReleaseNotesSettings.Tag))]
    public static T ResetTag<T>(this T settings)
        where T : GitHubReleaseNotesSettings
        => settings.Modify(
            x => x.Remove(
                () => settings.Tag));

    /// <summary>
    ///     Sets the commitish value that will be the target for the release's tag.
    /// </summary>
    /// <param name="settings">The settings instance to adjust.</param>
    /// <param name="targetCommitish">The commitish value that will be the target for the release's tag.</param>
    [Builder(
        Type = typeof(GitHubReleaseNotesSettings),
        Property = nameof(GitHubReleaseNotesSettings.TargetCommitish))]
    public static T SetTargetCommitish<T>(this T settings, string targetCommitish)
        where T : GitHubReleaseNotesSettings
        => settings.Modify(
            x => x.Set(
                () => settings.TargetCommitish,
                targetCommitish));

    /// <summary>
    ///     Resets the commitish value for the release's tag to <c>null</c>.
    /// </summary>
    /// <param name="settings">The settings instance to adjust.</param>
    [Builder(
        Type = typeof(GitHubReleaseNotesSettings),
        Property = nameof(GitHubReleaseNotesSettings.TargetCommitish))]
    public static T ResetTargetCommitish<T>(this T settings)
        where T : GitHubReleaseNotesSettings
        => settings.Modify(
            x => x.Remove(
                () => settings.TargetCommitish));

    /// <summary>
    ///     Sets the name of the previous tag to use as the starting point for the release notes.
    /// </summary>
    /// <param name="settings">The settings instance to adjust.</param>
    /// <param name="previousTag">The name of the previous tag to use as the starting point for the release notes.</param>
    [Builder(
        Type = typeof(GitHubReleaseNotesSettings),
        Property = nameof(GitHubReleaseNotesSettings.PreviousTag))]
    public static T SetPreviousTag<T>(this T settings, string previousTag)
        where T : GitHubReleaseNotesSettings
        => settings.Modify(
            x => x.Set(
                () => settings.PreviousTag,
                previousTag));

    /// <summary>
    ///     Resets the name of the previous tag to use to <c>null</c>.
    /// </summary>
    /// <param name="settings">The settings instance to adjust.</param>
    [Builder(
        Type = typeof(GitHubReleaseNotesSettings),
        Property = nameof(GitHubReleaseNotesSettings.PreviousTag))]
    public static T ResetPreviousTag<T>(this T settings)
        where T : GitHubReleaseNotesSettings
        => settings.Modify(
            x => x.Remove(
                () => settings.PreviousTag));

    /// <summary>
    ///     Sets the path to a file in the repository containing configuration settings used for generating the
    ///     release notes.
    /// </summary>
    /// <param name="settings">The settings instance to adjust.</param>
    /// <param name="configFile">
    ///     The optional path to a file in the repository containing configuration settings used for generating the
    ///     release notes.
    /// </param>
    [Builder(
        Type = typeof(GitHubReleaseNotesSettings),
        Property = nameof(GitHubReleaseNotesSettings.ConfigFile))]
    public static T SetConfigFile<T>(this T settings, string configFile)
        where T : GitHubReleaseNotesSettings
        => settings.Modify(
            x => x.Set(
                () => settings.ConfigFile,
                configFile));

    /// <summary>
    ///     Resets the path to a file in the repository containing configuration settings used for generating the release notes
    ///     to <c>null</c>.
    /// </summary>
    /// <param name="settings">The settings instance to adjust.</param>
    [Builder(
        Type = typeof(GitHubReleaseNotesSettings),
        Property = nameof(GitHubReleaseNotesSettings.ConfigFile))]
    public static T ResetConfigFile<T>(this T settings)
        where T : GitHubReleaseNotesSettings
        => settings.Modify(
            x => x.Remove(
                () => settings.ConfigFile));
}
