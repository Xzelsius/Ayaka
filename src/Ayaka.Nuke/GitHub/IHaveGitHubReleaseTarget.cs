// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.Nuke.GitHub;

using global::Nuke.Common;

/// <summary>
///     Indicates that a <see cref="INukeBuild" /> has a target for creating a GitHub release.
/// </summary>
public interface IHaveGitHubReleaseTarget
    : IHave
{
    /// <summary>
    ///     Gets the target for creating a GitHub release.
    /// </summary>
    Target GitHubRelease { get; }
}
