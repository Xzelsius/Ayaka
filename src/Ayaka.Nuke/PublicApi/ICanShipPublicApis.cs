// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.Nuke.PublicApi;

using System.Text;
using global::Nuke.Common;
using global::Nuke.Common.IO;
using global::Nuke.Common.ProjectModel;
using Serilog;

/// <summary>
///     Provides a target for shipping the Public API of all projects that track the Public API to the
///     <see cref="INukeBuild" />.
/// </summary>
public interface ICanShipPublicApis
    : ICan,
        IHaveSolution,
        IHaveSources,
        IHaveShipPublicApisTarget
{
    /// <summary>
    ///     Gets all projects that track the Public API.
    /// </summary>
    /// <remarks>
    ///     If not overridden, all projects in the <see cref="IHaveSources.SourceDirectory" /> are considered tracking the
    ///     Public API.
    /// </remarks>
    [ExcludeFromCodeCoverage]
    IEnumerable<Project> PublicApiProjects
        => Solution.AllProjects.Where(x => SourceDirectory.Contains(x.Path));

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    Target IHaveShipPublicApisTarget.ShipPublicApis => target => target
        .Description("Ship Public API of all Projects that track Public API")
        .Unlisted()
        .Executes(async () =>
        {
            var projects = PublicApiProjects.ToList();

            if (projects.Count == 0)
            {
                Log.Information("No projects found that track the Public API");
                return;
            }

            foreach (var project in PublicApiProjects)
            {
                Log.Information("Shipping Public API for {Project}", project.Name);

                var baselineFile = project.Directory / "PublicAPI.Shipped.txt";
                Log.Debug("Using {BaselineFile} as baseline", baselineFile);

                var additionsFile = project.Directory / "PublicAPI.Unshipped.txt";
                Log.Debug("Using {AdditionsFile} for additions", additionsFile);

                if (!File.Exists(baselineFile) || !File.Exists(additionsFile))
                {
                    Log.Warning("Baseline or additions file found. Skipping");
                    continue;
                }

                var baseline = (await File.ReadAllLinesAsync(baselineFile)).ToList();
                var additions = (await File.ReadAllLinesAsync(additionsFile)).ToList();

                var index = additions.FirstOrDefault() == "#nullable enable" ? 1 : 0;
                var edited = false;

                while (additions.Count > index)
                {
                    var addition = additions[index];

                    Log.Verbose("Shipping '{Addition}'", addition);

                    additions.RemoveAt(index);
                    baseline.Add(addition);

                    edited = true;
                }

                if (edited)
                {
                    baseline.Sort(IgnoreCaseWhenPossibleComparer.Instance);
                    additions.Sort(IgnoreCaseWhenPossibleComparer.Instance);

                    Log.Verbose("Persisting new baseline file");
                    await File.WriteAllLinesAsync(baselineFile, baseline, Encoding.UTF8);

                    Log.Verbose("Persisting new additions file");
                    await File.WriteAllLinesAsync(additionsFile, additions, Encoding.UTF8);
                }

                Log.Information("Completed shipping Public API for {Project}", project.Name);
            }
        });
}
