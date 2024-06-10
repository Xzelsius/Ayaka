// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.Nuke;

using global::Nuke.Common;
using global::Nuke.Common.ProjectModel;

/// <summary>
///     Provides information about the current solution in a <see cref="INukeBuild" />.
/// </summary>
public interface IHaveSolution : IHave
{
    /// <summary>
    ///     Gets the current solution.
    /// </summary>
    [Solution]
    [Required]
    [ExcludeFromCodeCoverage]
    Solution Solution => TryGetValue(() => Solution)!;
}
