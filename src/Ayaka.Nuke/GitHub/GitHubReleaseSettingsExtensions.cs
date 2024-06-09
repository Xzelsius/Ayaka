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
    public static T SetTag<T>(this T settings, string tagName)
        where T : GitHubReleaseSettings
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
        where T : GitHubReleaseSettings
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
        where T : GitHubReleaseSettings
    {
        settings = settings.NewInstance();
        settings.TargetCommitish = targetCommitish;

        return settings;
    }

    /// <summary>
    ///     Resets the commitish value to <c>null</c>.
    /// </summary>
    /// <param name="settings">The settings instance to adjust.</param>
    public static T ResetTargetCommitish<T>(this T settings)
        where T : GitHubReleaseSettings
    {
        settings = settings.NewInstance();
        settings.TargetCommitish = null;

        return settings;
    }

    /// <summary>
    ///     Sets the name of the release.
    /// </summary>
    /// <param name="settings">The settings instance to adjust.</param>
    /// <param name="name">The name of the release.</param>
    public static T SetName<T>(this T settings, string name)
        where T : GitHubReleaseSettings
    {
        settings = settings.NewInstance();
        settings.Name = name;

        return settings;
    }

    /// <summary>
    ///     Resets the name of the release to <c>null</c>.
    /// </summary>
    /// <param name="settings">The settings instance to adjust.</param>
    public static T ResetName<T>(this T settings)
        where T : GitHubReleaseSettings
    {
        settings = settings.NewInstance();
        settings.Name = null;

        return settings;
    }

    /// <summary>
    ///     Sets the text describing the contents of the release.
    /// </summary>
    /// <param name="settings">The settings instance to adjust.</param>
    /// <param name="body">The text describing the contents of the release.</param>
    public static T SetBody<T>(this T settings, string body)
        where T : GitHubReleaseSettings
    {
        settings = settings.NewInstance();
        settings.Body = body;

        return settings;
    }

    /// <summary>
    ///     Resets the text describing the contents of the release to <c>null</c>.
    /// </summary>
    /// <param name="settings">The settings instance to adjust.</param>
    public static T ResetBody<T>(this T settings)
        where T : GitHubReleaseSettings
    {
        settings = settings.NewInstance();
        settings.Body = null;

        return settings;
    }

    /// <summary>
    ///     Sets the value indicating whether the release is a draft.
    /// </summary>
    /// <param name="settings">The settings instance to adjust.</param>
    /// <param name="draft">The value indicating whether the release is a draft.</param>
    public static T SetDraft<T>(this T settings, bool draft)
        where T : GitHubReleaseSettings
    {
        settings = settings.NewInstance();
        settings.Draft = draft;

        return settings;
    }

    /// <summary>
    ///     Resets the value indicating whether the release is a draft to <c>null</c>.
    /// </summary>
    /// <param name="settings">The settings instance to adjust.</param>
    public static T ResetDraft<T>(this T settings)
        where T : GitHubReleaseSettings
    {
        settings = settings.NewInstance();
        settings.Draft = null;

        return settings;
    }

    /// <summary>
    ///     Sets the value indicating whether the release is a pre-release.
    /// </summary>
    /// <param name="settings">The settings instance to adjust.</param>
    /// <param name="preRelease">The value indicating whether the release is a pre-release.</param>
    public static T SetPreRelease<T>(this T settings, bool preRelease)
        where T : GitHubReleaseSettings
    {
        settings = settings.NewInstance();
        settings.PreRelease = preRelease;

        return settings;
    }

    /// <summary>
    ///     Resets the value indicating whether the release is a pre-release to <c>null</c>.
    /// </summary>
    /// <param name="settings">The settings instance to adjust.</param>
    public static T ResetPreRelease<T>(this T settings)
        where T : GitHubReleaseSettings
    {
        settings = settings.NewInstance();
        settings.PreRelease = null;

        return settings;
    }

    /// <summary>
    ///     Sets the value indicating whether to automatically generate release notes.
    /// </summary>
    /// <param name="settings">The settings instance to adjust.</param>
    /// <param name="generateReleaseNotes">The value indicating whether to automatically generate release notes.</param>
    public static T SetGenerateReleaseNotes<T>(this T settings, bool generateReleaseNotes)
        where T : GitHubReleaseSettings
    {
        settings = settings.NewInstance();
        settings.GenerateReleaseNotes = generateReleaseNotes;

        return settings;
    }

    /// <summary>
    ///     Resets the value indicating whether to automatically generate release notes to <c>null</c>.
    /// </summary>
    /// <param name="settings">The settings instance to adjust.</param>
    public static T ResetGenerateReleaseNotes<T>(this T settings)
        where T : GitHubReleaseSettings
    {
        settings = settings.NewInstance();
        settings.GenerateReleaseNotes = null;

        return settings;
    }

    /// <summary>
    ///     Adds a new artifact path to the release's artifacts.
    /// </summary>
    /// <param name="settings">The settings instance to adjust.</param>
    /// <param name="artifactPath">The artifact path to add to the release's artifacts.</param>
    public static T AddArtifactPath<T>(this T settings, string artifactPath)
        where T : GitHubReleaseSettings
    {
        settings = settings.NewInstance();
        settings.ArtifactPathsInternal.Add(artifactPath);

        return settings;
    }

    /// <summary>
    ///     Clears all artifact paths from the release's artifacts.
    /// </summary>
    /// <param name="settings">The settings instance to adjust.</param>
    public static T ClearArtifactPaths<T>(this T settings)
        where T : GitHubReleaseSettings
    {
        settings = settings.NewInstance();
        settings.ArtifactPathsInternal.Clear();

        return settings;
    }
}
