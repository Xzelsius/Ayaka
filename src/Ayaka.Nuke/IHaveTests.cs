// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.Nuke;

using global::Nuke.Common.IO;

/// <summary>
///     Provides information about the directory where test files are located in a <see cref="INukeBuild" />.
/// </summary>
public interface IHaveTests : IHave
{
    /// <summary>
    ///     Gets the absolute path to the directory where test files are located.
    /// </summary>
    /// <remarks>
    ///     If not overriden, defaults to <c><see cref="IHave.RootDirectory" />/tests</c>.
    /// </remarks>
    [ExcludeFromCodeCoverage]
    AbsolutePath TestsDirectory => RootDirectory / "test";
}
