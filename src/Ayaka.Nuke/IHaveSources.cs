// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.Nuke;

using global::Nuke.Common;
using global::Nuke.Common.IO;

/// <summary>
///     Provides information about the directory where source files are located in a <see cref="INukeBuild" />.
/// </summary>
public interface IHaveSources : IHave
{
    /// <summary>
    ///     Gets the absolute path to the directory where source files are located.
    /// </summary>
    /// <remarks>
    ///     If not overriden, defaults to <c><see cref="IHave.RootDirectory" />/src</c>.
    /// </remarks>
    [ExcludeFromCodeCoverage]
    AbsolutePath SourceDirectory => RootDirectory / "src";
}
