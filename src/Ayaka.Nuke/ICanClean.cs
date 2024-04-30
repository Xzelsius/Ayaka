// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.Nuke;

using global::Nuke.Common;
using global::Nuke.Common.IO;
using Serilog;

/// <summary>
///     Provides a target for cleaning up directories to the <see cref="INukeBuild" />.
/// </summary>
/// <remarks>
///     Supports <see cref="IHaveSources" />, <see cref="IHaveTests" />, and <see cref="IHaveArtifacts" />.
/// </remarks>
public interface ICanClean
    : ICan,
        IHaveCleanTarget
{
    /// <inheritdoc />
    Target IHaveCleanTarget.Clean => target => target
        .Description("Cleans up various build directories")
        .Executes(() =>
        {
            if (this is IHaveSources hasSources)
            {
                Log.Information("Cleaning up binaries in {Directory}", hasSources.SourceDirectory);

                hasSources.SourceDirectory.GlobDirectories("**/bin", "**/obj").DeleteDirectories();
            }

            if (this is IHaveTests hasTests)
            {
                Log.Information("Cleaning up binaries in {Directory}", hasTests.TestsDirectory);

                hasTests.TestsDirectory.GlobDirectories("**/bin", "**/obj").DeleteDirectories();
            }

            if (this is IHaveArtifacts hasArtifacts)
            {
                Log.Information("Cleaning up artifacts in {Directory}", hasArtifacts.ArtifactsDirectory);

                _ = hasArtifacts.ArtifactsDirectory.CreateOrCleanDirectory();
            }
        });
}
