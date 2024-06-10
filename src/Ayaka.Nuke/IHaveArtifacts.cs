// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.Nuke;

using global::Nuke.Common;
using global::Nuke.Common.IO;

/// <summary>
///     Provides information about the directory where artifacts are to be dropped in a <see cref="INukeBuild" />.
/// </summary>
public interface IHaveArtifacts : IHave
{
    /// <summary>
    ///     Gets the absolute path to the directory where artifacts are to be dropped.
    /// </summary>
    /// <remarks>
    ///     If not overriden, defaults to <c><see cref="INukeBuild.RootDirectory" />/artifacts</c>.
    /// </remarks>
    [Parameter("The directory where artifacts are to be dropped", Name = "Artifacts")]
    [ExcludeFromCodeCoverage]
    AbsolutePath ArtifactsDirectory => TryGetValue(() => ArtifactsDirectory)
                                       ?? RootDirectory / "artifacts";
}
