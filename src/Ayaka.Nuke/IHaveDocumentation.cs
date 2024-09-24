// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.Nuke;

using global::Nuke.Common;
using global::Nuke.Common.IO;

/// <summary>
///     Provides information about the directory where documentation files are located in a <see cref="INukeBuild" />.
/// </summary>
public interface IHaveDocumentation : IHave
{
    /// <summary>
    ///     Gets the absolute path to the directory where documentation files are located.
    /// </summary>
    /// <remarks>
    ///     If not override, default tos <c><see cref="IHave.RootDirectory" />/docs</c>.
    /// </remarks>
    [ExcludeFromCodeCoverage]
    AbsolutePath DocsDirectory => RootDirectory / "docs";
}
