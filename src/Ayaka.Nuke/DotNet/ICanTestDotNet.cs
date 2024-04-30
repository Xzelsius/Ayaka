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
using static global::Nuke.Common.Tools.DotNet.DotNetTasks;

/// <summary>
///     Provides a target for testing all projects in the <see cref="IHaveTests.TestsDirectory" /> using .NET CLI to the
///     <see cref="INukeBuild" />.
/// </summary>
/// <remarks>
///     Automatically collects code coverage during test if <see cref="IHaveCodeCoverage"/> is implemented.
/// </remarks>
public interface ICanTestDotNet
    : ICan,
        IHaveTestArtifacts,
        IHaveSolution,
        IHaveTests,
        IHaveConfiguration,
        IHaveCompileTarget,
        IHaveTestTarget
{
    /// <summary>
    ///     Gets the base settings for testing a project.
    /// </summary>
    sealed Configure<DotNetTestSettings> TestSettingsBase
        => dotnet => dotnet
            .SetConfiguration(Configuration)
            .SetNoBuild(SucceededTargets.Contains(Compile))
            .ResetVerbosity()
            .SetResultsDirectory(TestResultsDirectory);

    /// <summary>
    ///     Gets the additional settings for testing a project.
    /// </summary>
    /// <remarks>
    ///     Override this to provide additional settings for the <see cref="IHaveTestTarget.Test" /> target.
    /// </remarks>
    Configure<DotNetTestSettings> TestSettings
        => dotnet => dotnet;

    /// <summary>
    ///     Gets the base settings for testing a project scoped to one specific project.
    /// </summary>
    sealed Configure<DotNetTestSettings, Project> TestProjectSettingsBase
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
    ///     Override this to provide additional settings for the <see cref="IHaveTestTarget.Test" /> target.
    /// </remarks>
    Configure<DotNetTestSettings, Project> TestProjectSettings
        => (dotnet, project) => dotnet;

    /// <summary>
    ///     Gets all test projects.
    /// </summary>
    /// <remarks>
    ///     If not overridden, all projects in the <see cref="IHaveTests.TestsDirectory" /> are considered test projects.
    /// </remarks>
    IEnumerable<Project> TestProjects
        => Solution
            .AllProjects
            .Where(x => TestsDirectory.Contains(x.Path));

    /// <inheritdoc />
    Target IHaveTestTarget.Test => target => target
        .Description("Tests all test projects using .NET CLI")
        .DependsOn(Compile)
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
                _ = DotNetTest(
                    dotnet => dotnet
                        .Apply(TestSettingsBase)
                        .Apply(TestSettings)
                        .CombineWith(
                            TestProjects,
                            (d, p) => d
                                .Apply(TestProjectSettingsBase, p)
                                .Apply(TestProjectSettings, p)),
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
