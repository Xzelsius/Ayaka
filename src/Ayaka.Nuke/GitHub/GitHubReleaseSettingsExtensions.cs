// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.Nuke.GitHub;

using global::Nuke.Common.Tooling;

/// <summary>
///     Extension methods for <see cref="GitHubReleaseSettings" />
/// </summary>
public static class GitHubReleaseSettingsExtensions
{
    /// <summary>
    ///     Sets the tag name for the release.
    /// </summary>
    /// <param name="settings">The settings instance to adjust.</param>
    /// <param name="tagName">The tag name for the release.</param>
    [Builder(
        Type = typeof(GitHubReleaseSettings),
        Property = nameof(GitHubReleaseSettings.Tag))]
    public static T SetTag<T>(this T settings, string tagName)
        where T : GitHubReleaseSettings
        => settings.Modify(
            x => x.Set(
                () => settings.Tag,
                tagName));

    /// <summary>
    ///     Resets the tag name for the release to <c>null</c>.
    /// </summary>
    /// <param name="settings">The settings instance to adjust.</param>
    [Builder(
        Type = typeof(GitHubReleaseSettings),
        Property = nameof(GitHubReleaseSettings.Tag))]
    public static T ResetTag<T>(this T settings)
        where T : GitHubReleaseSettings
        => settings.Modify(
            x => x.Remove(
                () => settings.Tag));

    /// <summary>
    ///     Sets the commitish value that will be the target for the release's tag.
    /// </summary>
    /// <param name="settings">The settings instance to adjust.</param>
    /// <param name="targetCommitish">The commitish value that will be the target for the release's tag.</param>
    [Builder(
        Type = typeof(GitHubReleaseSettings),
        Property = nameof(GitHubReleaseSettings.TargetCommitish))]
    public static T SetTargetCommitish<T>(this T settings, string targetCommitish)
        where T : GitHubReleaseSettings
        => settings.Modify(
            x => x.Set(
                () => settings.TargetCommitish,
                targetCommitish));

    /// <summary>
    ///     Resets the commitish value to <c>null</c>.
    /// </summary>
    /// <param name="settings">The settings instance to adjust.</param>
    [Builder(
        Type = typeof(GitHubReleaseSettings),
        Property = nameof(GitHubReleaseSettings.TargetCommitish))]
    public static T ResetTargetCommitish<T>(this T settings)
        where T : GitHubReleaseSettings
        => settings.Modify(
            x => x.Remove(
                () => settings.TargetCommitish));

    /// <summary>
    ///     Sets the name of the release.
    /// </summary>
    /// <param name="settings">The settings instance to adjust.</param>
    /// <param name="name">The name of the release.</param>
    [Builder(
        Type = typeof(GitHubReleaseSettings),
        Property = nameof(GitHubReleaseSettings.Name))]
    public static T SetName<T>(this T settings, string name)
        where T : GitHubReleaseSettings
        => settings.Modify(
            x => x.Set(
                () => settings.Name,
                name));

    /// <summary>
    ///     Resets the name of the release to <c>null</c>.
    /// </summary>
    /// <param name="settings">The settings instance to adjust.</param>
    [Builder(
        Type = typeof(GitHubReleaseSettings),
        Property = nameof(GitHubReleaseSettings.Name))]
    public static T ResetName<T>(this T settings)
        where T : GitHubReleaseSettings
        => settings.Modify(
            x => x.Remove(
                () => settings.Name));

    /// <summary>
    ///     Sets the text describing the contents of the release.
    /// </summary>
    /// <param name="settings">The settings instance to adjust.</param>
    /// <param name="body">The text describing the contents of the release.</param>
    [Builder(
        Type = typeof(GitHubReleaseSettings),
        Property = nameof(GitHubReleaseSettings.Body))]
    public static T SetBody<T>(this T settings, string body)
        where T : GitHubReleaseSettings
        => settings.Modify(
            x => x.Set(
                () => settings.Body,
                body));

    /// <summary>
    ///     Resets the text describing the contents of the release to <c>null</c>.
    /// </summary>
    /// <param name="settings">The settings instance to adjust.</param>
    [Builder(
        Type = typeof(GitHubReleaseSettings),
        Property = nameof(GitHubReleaseSettings.Body))]
    public static T ResetBody<T>(this T settings)
        where T : GitHubReleaseSettings
        => settings.Modify(
            x => x.Remove(
                () => settings.Body));

    /// <summary>
    ///     Sets the value indicating whether the release is a draft.
    /// </summary>
    /// <param name="settings">The settings instance to adjust.</param>
    /// <param name="draft">The value indicating whether the release is a draft.</param>
    [Builder(
        Type = typeof(GitHubReleaseSettings),
        Property = nameof(GitHubReleaseSettings.Draft))]
    public static T SetDraft<T>(this T settings, bool draft)
        where T : GitHubReleaseSettings
        => settings.Modify(
            x => x.Set(
                () => settings.Draft,
                draft));

    /// <summary>
    ///     Resets the value indicating whether the release is a draft to <c>null</c>.
    /// </summary>
    /// <param name="settings">The settings instance to adjust.</param>
    [Builder(
        Type = typeof(GitHubReleaseSettings),
        Property = nameof(GitHubReleaseSettings.Draft))]
    public static T ResetDraft<T>(this T settings)
        where T : GitHubReleaseSettings
        => settings.Modify(
            x => x.Remove(
                () => settings.Draft));

    /// <summary>
    ///     Sets the value indicating whether the release is a pre-release.
    /// </summary>
    /// <param name="settings">The settings instance to adjust.</param>
    /// <param name="preRelease">The value indicating whether the release is a pre-release.</param>
    [Builder(
        Type = typeof(GitHubReleaseSettings),
        Property = nameof(GitHubReleaseSettings.PreRelease))]
    public static T SetPreRelease<T>(this T settings, bool preRelease)
        where T : GitHubReleaseSettings
        => settings.Modify(
            x => x.Set(
                () => settings.PreRelease,
                preRelease));

    /// <summary>
    ///     Resets the value indicating whether the release is a pre-release to <c>null</c>.
    /// </summary>
    /// <param name="settings">The settings instance to adjust.</param>
    [Builder(
        Type = typeof(GitHubReleaseSettings),
        Property = nameof(GitHubReleaseSettings.PreRelease))]
    public static T ResetPreRelease<T>(this T settings)
        where T : GitHubReleaseSettings
        => settings.Modify(
            x => x.Remove(
                () => settings.PreRelease));

    /// <summary>
    ///     Sets the value indicating whether to automatically generate release notes.
    /// </summary>
    /// <param name="settings">The settings instance to adjust.</param>
    /// <param name="generateReleaseNotes">The value indicating whether to automatically generate release notes.</param>
    [Builder(
        Type = typeof(GitHubReleaseSettings),
        Property = nameof(GitHubReleaseSettings.GenerateReleaseNotes))]
    public static T SetGenerateReleaseNotes<T>(this T settings, bool generateReleaseNotes)
        where T : GitHubReleaseSettings
        => settings.Modify(
            x => x.Set(
                () => settings.GenerateReleaseNotes,
                generateReleaseNotes));

    /// <summary>
    ///     Resets the value indicating whether to automatically generate release notes to <c>null</c>.
    /// </summary>
    /// <param name="settings">The settings instance to adjust.</param>
    [Builder(
        Type = typeof(GitHubReleaseSettings),
        Property = nameof(GitHubReleaseSettings.GenerateReleaseNotes))]
    public static T ResetGenerateReleaseNotes<T>(this T settings)
        where T : GitHubReleaseSettings
        => settings.Modify(
            x => x.Remove(
                () => settings.GenerateReleaseNotes));

    /// <summary>
    ///     Adds a new artifact path to the release's artifacts.
    /// </summary>
    /// <param name="settings">The settings instance to adjust.</param>
    /// <param name="artifactPath">The artifact path to add to the release's artifacts.</param>
    [Builder(
        Type = typeof(GitHubReleaseSettings),
        Property = nameof(GitHubReleaseSettings.ArtifactPaths))]
    public static T AddArtifactPath<T>(this T settings, string artifactPath)
        where T : GitHubReleaseSettings
        => settings.Modify(
            x => x.AddCollection(
                () => settings.ArtifactPaths,
                artifactPath));

    /// <summary>
    ///     Clears all artifact paths from the release's artifacts.
    /// </summary>
    /// <param name="settings">The settings instance to adjust.</param>
    [Builder(
        Type = typeof(GitHubReleaseSettings),
        Property = nameof(GitHubReleaseSettings.ArtifactPaths))]
    public static T ClearArtifactPaths<T>(this T settings)
        where T : GitHubReleaseSettings
        => settings.Modify(
            x => x.ClearCollection(
                () => settings.ArtifactPaths));
}
