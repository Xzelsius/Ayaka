// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.Nuke;

using global::Nuke.Common;
using global::Nuke.Common.Git;

/// <summary>
///     Provides information about the current git repository in a <see cref="INukeBuild" />.
/// </summary>
public interface IHaveGitRepository : IHave
{
    /// <summary>
    ///     Gets the current git repository
    /// </summary>
    [GitRepository]
    [Required]
    GitRepository GitRepository => TryGetValue(() => GitRepository)!;
}
