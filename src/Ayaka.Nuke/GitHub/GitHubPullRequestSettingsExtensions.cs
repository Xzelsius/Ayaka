// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.Nuke.GitHub;

using global::Nuke.Common.Tooling;

/// <summary>
///     Extension methods for <see cref="GitHubPullRequestSettings" />.
/// </summary>
public static class GitHubPullRequestSettingsExtensions
{
    /// <summary>
    ///     Sets the name of the branch where changes are implemented.
    /// </summary>
    /// <param name="settings">The settings instance to adjust.</param>
    /// <param name="head">The name of the branch where changes are implemented.</param>
    [Builder(
        Type = typeof(GitHubPullRequestSettings),
        Property = nameof(GitHubPullRequestSettings.Head))]
    public static T SetHead<T>(this T settings, string head)
        where T : GitHubPullRequestSettings
        => settings.Modify(
            x => x.Set(
                () => settings.Head,
                head));

    /// <summary>
    ///     Resets the name of the branch where changes are implemented to <c>null</c>.
    /// </summary>
    /// <param name="settings">The settings instance to adjust.</param>
    [Builder(
        Type = typeof(GitHubPullRequestSettings),
        Property = nameof(GitHubPullRequestSettings.Head))]
    public static T ResetHead<T>(this T settings)
        where T : GitHubPullRequestSettings
        => settings.Modify(
            x => x.Remove(
                () => settings.Head));

    /// <summary>
    ///     Sets the name of the branch where change should be pulled into.
    /// </summary>
    /// <param name="settings">The settings instance to adjust.</param>
    /// <param name="base">The name of the branch where change should be pulled into.</param>
    [Builder(
        Type = typeof(GitHubPullRequestSettings),
        Property = nameof(GitHubPullRequestSettings.Base))]
    public static T SetBase<T>(this T settings, string @base)
        where T : GitHubPullRequestSettings
        => settings.Modify(
            x => x.Set(
                () => settings.Base,
                @base));

    /// <summary>
    ///     Resets the name of the branch where change should be pulled into to <c>null</c>.
    /// </summary>
    /// <param name="settings">The settings instance to adjust.</param>
    [Builder(
        Type = typeof(GitHubPullRequestSettings),
        Property = nameof(GitHubPullRequestSettings.Base))]
    public static T ResetBase<T>(this T settings)
        where T : GitHubPullRequestSettings
        => settings.Modify(
            x => x.Remove(
                () => settings.Base));

    /// <summary>
    ///     Sets the title of the pull request.
    /// </summary>
    /// <param name="settings">The settings instance to adjust.</param>
    /// <param name="title">The title of the pull request.</param>
    [Builder(
        Type = typeof(GitHubPullRequestSettings),
        Property = nameof(GitHubPullRequestSettings.Title))]
    public static T SetTitle<T>(this T settings, string title)
        where T : GitHubPullRequestSettings
        => settings.Modify(
            x => x.Set(
                () => settings.Title,
                title));

    /// <summary>
    ///     Resets the title of the pull request to <c>null</c>.
    /// </summary>
    /// <param name="settings">The settings instance to adjust.</param>
    [Builder(
        Type = typeof(GitHubPullRequestSettings),
        Property = nameof(GitHubPullRequestSettings.Title))]
    public static T ResetTitle<T>(this T settings)
        where T : GitHubPullRequestSettings
        => settings.Modify(
            x => x.Remove(
                () => settings.Title));

    /// <summary>
    ///     Sets the text describing the contents of the pull request.
    /// </summary>
    /// <param name="settings">The settings instance to adjust.</param>
    /// <param name="body">The body of the pull request.</param>
    [Builder(
        Type = typeof(GitHubPullRequestSettings),
        Property = nameof(GitHubPullRequestSettings.Body))]
    public static T SetBody<T>(this T settings, string body)
        where T : GitHubPullRequestSettings
        => settings.Modify(
            x => x.Set(
                () => settings.Body,
                body));

    /// <summary>
    ///     Resets the text describing the contents of the pull request to <c>null</c>.
    /// </summary>
    /// <param name="settings">The settings instance to adjust.</param>
    [Builder(
        Type = typeof(GitHubPullRequestSettings),
        Property = nameof(GitHubPullRequestSettings.Body))]
    public static T ResetBody<T>(this T settings)
        where T : GitHubPullRequestSettings
        => settings.Modify(
            x => x.Remove(
                () => settings.Body));

    /// <summary>
    ///     Sets the value indicating whether the pull request is a draft.
    /// </summary>
    /// <param name="settings">The settings instance to adjust.</param>
    /// <param name="draft">The value indicating whether the pull request is a draft.</param>
    [Builder(
        Type = typeof(GitHubPullRequestSettings),
        Property = nameof(GitHubPullRequestSettings.Draft))]
    public static T SetDraft<T>(this T settings, bool draft)
        where T : GitHubPullRequestSettings
        => settings.Modify(
            x => x.Set(
                () => settings.Draft,
                draft));

    /// <summary>
    ///     Resets the value indicating whether the pull request is a draft to <c>null</c>.
    /// </summary>
    /// <param name="settings">The settings instance to adjust.</param>
    [Builder(
        Type = typeof(GitHubPullRequestSettings),
        Property = nameof(GitHubPullRequestSettings.Draft))]
    public static T ResetDraft<T>(this T settings)
        where T : GitHubPullRequestSettings
        => settings.Modify(
            x => x.Remove(
                () => settings.Draft));
}
