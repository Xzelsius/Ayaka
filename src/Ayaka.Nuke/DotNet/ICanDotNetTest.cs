// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.Nuke.DotNet;

using global::Nuke.Common;
using global::Nuke.Common.CI.GitHubActions;
using global::Nuke.Common.IO;
using global::Nuke.Common.ProjectModel;
using global::Nuke.Common.Tooling;
using global::Nuke.Common.Tools.Coverlet;
using global::Nuke.Common.Tools.DotNet;
using global::Nuke.Common.Utilities.Collections;

/// <summary>
///     Provides a target for testing all projects in the <see cref="IHaveTests.TestsDirectory" /> using .NET CLI to the
///     <see cref="INukeBuild" />.
/// </summary>
/// <remarks>
///     Automatically collects code coverage during test if <see cref="IHaveCodeCoverage" /> is implemented.
/// </remarks>
public interface ICanDotNetTest
    : ICan,
        IHaveTestArtifacts,
        IHaveSolution,
        IHaveTests,
        IHaveDotNetConfiguration,
        IHaveDotNetBuildTarget,
        IHaveDotNetTestTarget
{
    /// <summary>
    ///     Gets the base settings for testing a project.
    /// </summary>
    [ExcludeFromCodeCoverage]
    sealed Configure<DotNetTestSettings> DotNetTestSettingsBase
        => dotnet => dotnet
            .SetConfiguration(DotNetConfiguration)
            .SetNoBuild(SucceededTargets.Contains(DotNetBuild))
            .ResetVerbosity()
            .SetResultsDirectory(TestResultsDirectory);

    /// <summary>
    ///     Gets the additional settings for testing a project.
    /// </summary>
    /// <remarks>
    ///     Override this to provide additional settings for the <see cref="IHaveDotNetTestTarget.DotNetTest" /> target.
    /// </remarks>
    [ExcludeFromCodeCoverage]
    Configure<DotNetTestSettings> DotNetTestSettings
        => dotnet => dotnet;

    /// <summary>
    ///     Gets the base settings for testing a project scoped to one specific project.
    /// </summary>
    [ExcludeFromCodeCoverage]
    sealed Configure<DotNetTestSettings, Project> DotNetTestProjectSettingsBase
        => (dotnet, project) => dotnet
            .SetProjectFile(project)
            .When(
                GitHubActions.Instance is not null && project.HasPackageReference("GitHubActionsTestLogger"),
                d => d.AddLoggers("GitHubActions;report-warnings=false"))
            .AddLoggers($"trx;LogFileName={project.Name}.trx")
            .When(
                this is IHaveCodeCoverage && project.HasPackageReference("coverlet.collector"),
                d => d
                    .SetDataCollector("XPlat Code Coverage")
                    .EnableUseSourceLink());

    /// <summary>
    ///     Gets the additional settings for testing a project scoped to one specific project.
    /// </summary>
    /// <remarks>
    ///     Override this to provide additional settings for the <see cref="IHaveDotNetTestTarget.DotNetTest" /> target.
    /// </remarks>
    [ExcludeFromCodeCoverage]
    Configure<DotNetTestSettings, Project> DotNetTestProjectSettings
        => (dotnet, project) => dotnet;

    /// <summary>
    ///     Gets all test projects.
    /// </summary>
    /// <remarks>
    ///     If not overridden, all projects in the <see cref="IHaveTests.TestsDirectory" /> are considered test projects.
    /// </remarks>
    [ExcludeFromCodeCoverage]
    IEnumerable<Project> TestProjects
        => Solution
            .AllProjects
            .Where(x => TestsDirectory.Contains(x.Path));

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    Target IHaveDotNetTestTarget.DotNetTest => target => target
        .Description("Tests all test projects using .NET CLI")
        .Unlisted()
        .DependsOn(DotNetBuild)
        .Produces(TestResultsDirectory / ".trx")
        .Produces(TestResultsDirectory / ".xml")
        .WhenNotNull(
            this as IHaveCodeCoverage,
            (t, o) => t
                .Produces(o.CoverageDirectory / ".xml"))
        .Executes(() =>
        {
            try
            {
                _ = DotNetTasks.DotNetTest(
                    dotnet => dotnet
                        .Apply(DotNetTestSettingsBase)
                        .Apply(DotNetTestSettings)
                        .CombineWith(
                            TestProjects,
                            (d, p) => d
                                .Apply(DotNetTestProjectSettingsBase, p)
                                .Apply(DotNetTestProjectSettings, p)),
                    degreeOfParallelism: 1,
                    completeOnFailure: true
                );
            }
            finally
            {
                ReportTestCount();
            }
        })
        .WhenNotNull(
            this as IHaveCodeCoverage,
            (t, o) => t
                .CopyCoverageFiles(TestResultsDirectory, o.CoverageDirectory));

    /// <summary>
    ///     Writes the test results to the build report.
    /// </summary>
    /// <remarks>
    ///     Requires the test runner to use the <c>trx</c> logger.
    ///     <para>
    ///         Use <c>.AddLoggers($"trx;LogFileName={project.Name}.trx")</c> on the per-project test settings to enable this.
    ///     </para>
    /// </remarks>
    [ExcludeFromCodeCoverage]
    void ReportTestCount()
    {
        static IEnumerable<string> GetOutcomes(AbsolutePath file)
        {
            return XmlTasks.XmlPeek(
                file,
                "/xn:TestRun/xn:Results/xn:UnitTestResult/@outcome",
                ("xn", "http://microsoft.com/schemas/VisualStudio/TeamTest/2010"));
        }

        var resultFiles = TestResultsDirectory.GlobFiles("*.trx");
        var outcomes = resultFiles.SelectMany(GetOutcomes).ToList();
        var passedTests = outcomes.Count(x => x == "Passed");
        var failedTests = outcomes.Count(x => x == "Failed");
        var skippedTests = outcomes.Count(x => x == "NotExecuted");

        ReportSummary(
            summary => summary
                .When(failedTests > 0, s => s.AddPair("Failed", failedTests.ToString()))
                .AddPair("Passed", passedTests.ToString())
                .When(skippedTests > 0, s => s.AddPair("Skipped", skippedTests.ToString())));
    }
}
