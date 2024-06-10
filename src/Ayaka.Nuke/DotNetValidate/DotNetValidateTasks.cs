// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.Nuke.DotNetValidate;

using global::Nuke.Common.Tooling;

/// <summary>
///     Provides tasks for validating NuGet packages using <c>dotnet-validate</c> CLI tool.
/// </summary>
[NuGetPackageRequirement(DotNetValidatePackageId)]
[ExcludeFromCodeCoverage]
public class DotNetValidateTasks
    : IRequireNuGetPackage
{
    /// <summary>
    ///     The identifier of the <c>dotnet-validate</c> NuGet package.
    /// </summary>
    public const string DotNetValidatePackageId = "dotnet-validate";

    /// <summary>
    ///     Gets the path to the executable of <c>dotnet-validate</c>.
    /// </summary>
    /// <remarks>
    ///     Tries to resolve by environment variable <c>DOTNET_VALIDATE_EXE</c> and if not found, by <c>dotnet-validate.dll</c>
    ///     from within the NuGet package.
    /// </remarks>
    public static string DotNetValidatePath
        => ToolPathResolver.TryGetEnvironmentExecutable("DOTNET_VALIDATE_EXE")
           ?? NuGetToolPathResolver.GetPackageExecutable(DotNetValidatePackageId, "dotnet-validate.dll");

    /// <summary>
    ///     Gets or sets default process logger for <c>dotnet-validate</c>.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="ProcessTasks.DefaultLogger" />.
    /// </remarks>
    public static Action<OutputType, string> DotNetValidateLogger { get; set; } = ProcessTasks.DefaultLogger;

    /// <summary>
    ///     Gets or sets the default process exit handler for <c>dotnet-validate</c>.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="ProcessTasks.DefaultExitHandler" />.
    /// </remarks>
    public static Action<ToolSettings?, IProcess> DotNetValidateExitHandler { get; set; } =
        ProcessTasks.DefaultExitHandler;

    /// <summary>
    ///     Validates NuGet package health using <c>dotnet-validate</c> CLI tool.
    ///     Ensures your package meets the .NET foundation's guidelines for secure packages.
    /// </summary>
    /// <remarks>
    ///     For more details, visit the
    ///     <see href="https://github.com/NuGetPackageExplorer/NuGetPackageExplorer">official website</see>.
    /// </remarks>
    /// <param name="arguments">The process arguments.</param>
    /// <param name="workingDirectory">The working directory of the process.</param>
    /// <param name="environmentVariables">The environment variables to assign for the process.</param>
    /// <param name="timeout">The time to wait for the process to exit.</param>
    /// <param name="logOutput">Indicates whether to log output or not.</param>
    /// <param name="logInvocation">Indicates whether to log the invocation of the process.</param>
    /// <param name="logger">The logger to use for the process.</param>
    /// <param name="exitHandler">The exit handler to use for the process.</param>
    /// <returns>A <see cref="IReadOnlyCollection{Output}" /> containing the process output.</returns>
    public static IReadOnlyCollection<Output> DotNetValidate(
        ArgumentStringHandler arguments,
        string? workingDirectory = null,
        IReadOnlyDictionary<string, string>? environmentVariables = null,
        int? timeout = null,
        bool? logOutput = null,
        bool? logInvocation = null,
        Action<OutputType, string>? logger = null,
        Action<IProcess>? exitHandler = null)
    {
        using var process = ProcessTasks.StartProcess(
            DotNetValidatePath,
            arguments,
            workingDirectory,
            environmentVariables,
            timeout,
            logOutput,
            logInvocation,
            logger ?? DotNetValidateLogger);

        (exitHandler ?? (p => DotNetValidateExitHandler.Invoke(arg1: null, p)))
            .Invoke(process.AssertWaitForExit());

        return process.Output;
    }

    /// <summary>
    ///     Validates NuGet package health for a local package file using <c>dotnet-validate</c> CLI tool.
    ///     Ensures your package meets the .NET foundation's guidelines for secure packages.
    /// </summary>
    /// <remarks>
    ///     For more details, visit the
    ///     <see href="https://github.com/NuGetPackageExplorer/NuGetPackageExplorer">official website</see>.
    /// </remarks>
    /// <param name="toolSettings">The settings to use for the process.</param>
    /// <returns>A <see cref="IReadOnlyCollection{Output}" /> containing the process output.</returns>
    public static IReadOnlyCollection<Output> DotNetValidateLocalPackage(DotNetValidateLocalPackageSettings toolSettings)
    {
        using var process = ProcessTasks.StartProcess(toolSettings);

        toolSettings.ProcessExitHandler.Invoke(toolSettings, process.AssertWaitForExit());

        return process.Output;
    }

    /// <summary>
    ///     Validates NuGet package health for a local package file using <c>dotnet-validate</c> CLI tool.
    ///     Ensures your package meets the .NET foundation's guidelines for secure packages.
    /// </summary>
    /// <remarks>
    ///     For more details, visit the
    ///     <see href="https://github.com/NuGetPackageExplorer/NuGetPackageExplorer">official website</see>.
    /// </remarks>
    /// <param name="configurator">A method to use for configuring the setting of the process.</param>
    /// <returns>A <see cref="IReadOnlyCollection{Output}" /> containing the process output.</returns>
    public static IReadOnlyCollection<Output> DotNetValidateLocalPackage(Configure<DotNetValidateLocalPackageSettings> configurator)
        => DotNetValidateLocalPackage(configurator(new DotNetValidateLocalPackageSettings()));

    /// <summary>
    ///     Validates NuGet package health for a local package file using <c>dotnet-validate</c> CLI tool.
    ///     Ensures your package meets the .NET foundation's guidelines for secure packages.
    /// </summary>
    /// <remarks>
    ///     For more details, visit the
    ///     <see href="https://github.com/NuGetPackageExplorer/NuGetPackageExplorer">official website</see>.
    /// </remarks>
    /// <param name="configurator">A method to use for configuring settings of multiple invocations of the process.</param>
    /// <param name="degreeOfParallelism">The degree of parallelism when invoking the process.</param>
    /// <param name="completeOnFailure">Indicates whether continue invocation on failures or not.</param>
    /// <returns>A <see cref="IEnumerable{T}" /> containing the settings and output of each invocation of the process.</returns>
    public static
        IEnumerable<(DotNetValidateLocalPackageSettings Settings, IReadOnlyCollection<Output> Output)>
        DotNetValidateLocalPackage(
            CombinatorialConfigure<DotNetValidateLocalPackageSettings> configurator,
            int degreeOfParallelism = 1,
            bool completeOnFailure = false)
        => configurator.Invoke(
            DotNetValidateLocalPackage,
            DotNetValidateLogger,
            degreeOfParallelism,
            completeOnFailure);

    /// <summary>
    ///     Validates NuGet package health for a remote package using <c>dotnet-validate</c> CLI tool.
    ///     Ensures your package meets the .NET foundation's guidelines for secure packages.
    /// </summary>
    /// <remarks>
    ///     For more details, visit the
    ///     <see href="https://github.com/NuGetPackageExplorer/NuGetPackageExplorer">official website</see>.
    /// </remarks>
    /// <param name="toolSettings">The settings to use for the process.</param>
    /// <returns>A <see cref="IReadOnlyCollection{Output}" /> containing the process output.</returns>
    public static IReadOnlyCollection<Output> DotNetValidateRemotePackage(DotNetValidateRemotePackageSettings toolSettings)
    {
        using var process = ProcessTasks.StartProcess(toolSettings);

        toolSettings.ProcessExitHandler.Invoke(toolSettings, process.AssertWaitForExit());

        return process.Output;
    }

    /// <summary>
    ///     Validates NuGet package health for a remote package using <c>dotnet-validate</c> CLI tool.
    ///     Ensures your package meets the .NET foundation's guidelines for secure packages.
    /// </summary>
    /// <remarks>
    ///     For more details, visit the
    ///     <see href="https://github.com/NuGetPackageExplorer/NuGetPackageExplorer">official website</see>.
    /// </remarks>
    /// <param name="configurator">A method to use for configuring the setting of the process.</param>
    /// <returns>A <see cref="IReadOnlyCollection{Output}" /> containing the process output.</returns>
    public static IReadOnlyCollection<Output> DotNetValidateRemotePackage(Configure<DotNetValidateRemotePackageSettings> configurator)
        => DotNetValidateRemotePackage(configurator(new DotNetValidateRemotePackageSettings()));

    /// <summary>
    ///     Validates NuGet package health for a remote package using <c>dotnet-validate</c> CLI tool.
    ///     Ensures your package meets the .NET foundation's guidelines for secure packages.
    /// </summary>
    /// <remarks>
    ///     For more details, visit the
    ///     <see href="https://github.com/NuGetPackageExplorer/NuGetPackageExplorer">official website</see>.
    /// </remarks>
    /// <param name="configurator">A method to use for configuring settings of multiple invocations of the process.</param>
    /// <param name="degreeOfParallelism">The degree of parallelism when invoking the process.</param>
    /// <param name="completeOnFailure">Indicates whether continue invocation on failures or not.</param>
    /// <returns>A <see cref="IEnumerable{T}" /> containing the settings and output of each invocation of the process.</returns>
    public static
        IEnumerable<(DotNetValidateRemotePackageSettings Settings, IReadOnlyCollection<Output> Output)>
        DotNetValidateRemotePackage(
            CombinatorialConfigure<DotNetValidateRemotePackageSettings> configurator,
            int degreeOfParallelism = 1,
            bool completeOnFailure = false)
        => configurator.Invoke(
            DotNetValidateRemotePackage,
            DotNetValidateLogger,
            degreeOfParallelism,
            completeOnFailure);
}
