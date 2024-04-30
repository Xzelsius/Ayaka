// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.Nuke.DotNet;

using global::Nuke.Common;
using global::Nuke.Common.IO;
using static global::Nuke.Common.IO.FileSystemTasks;

internal static class TargetDefinitionExtensions
{
    /// <summary>
    ///     Copies the coverage files matching <c>*.cobertura.xml</c> from the <paramref name="testResultsDirectory" /> to the
    ///     <paramref name="coverageDirectory" />.
    /// </summary>
    /// <param name="targetDefinition">The target definition where the action should be executed.</param>
    /// <param name="testResultsDirectory">The path to the test artifacts.</param>
    /// <param name="coverageDirectory">The path to the coverage files.</param>
    /// <returns>The same target definition for chaining.</returns>
    public static ITargetDefinition CopyCoverageFiles(
        this ITargetDefinition targetDefinition,
        AbsolutePath testResultsDirectory,
        AbsolutePath coverageDirectory)
        => targetDefinition
            .Executes(() =>
            {
                var coverageFiles = testResultsDirectory
                    .GlobFiles("**/*.cobertura.xml")
                    // Only copy coverage files that have a parent directory with a GUID as name
                    // XPlat Code Coverage results in duplicates, we only copy the one with a GUID as directory name
                    .Where(x => x.Parent is not null && Guid.TryParse(x.Parent.Name, out _));

                foreach (var coverageFile in coverageFiles)
                {
                    var filename = $"{coverageFile.Parent!.Name}.cobertura.xml";

                    CopyFile(
                        coverageFile,
                        coverageDirectory / filename);
                }
            });
}
