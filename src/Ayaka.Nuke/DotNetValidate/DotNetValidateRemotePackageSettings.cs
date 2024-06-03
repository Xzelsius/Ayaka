// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.Nuke.DotNetValidate;

using global::Nuke.Common.Tooling;

/// <summary>
///     Provides settings for validating a remote NuGet package using <c>dotnet-validate</c> CLI tool.
/// </summary>
[Serializable]
public class DotNetValidateRemotePackageSettings : ToolSettings
{
    /// <summary>
    ///     Gets the path to the executable of <c>dotnet-validate</c>.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="DotNetValidateTasks.DotNetValidatePath" />.
    /// </remarks>
    public override string ProcessToolPath => base.ProcessToolPath ?? DotNetValidateTasks.DotNetValidatePath;

    /// <summary>
    ///     Gets the process logger
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="DotNetValidateTasks.DotNetValidateLogger" />.
    /// </remarks>
    public override Action<OutputType, string> ProcessLogger =>
        base.ProcessLogger ?? DotNetValidateTasks.DotNetValidateLogger;

    /// <summary>
    ///     Gets the process exit handler.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="DotNetValidateTasks.DotNetValidateExitHandler" />.
    /// </remarks>
    public override Action<ToolSettings, IProcess> ProcessExitHandler =>
        base.ProcessExitHandler ?? DotNetValidateTasks.DotNetValidateExitHandler;

    /// <summary>
    ///     Gets the identifier of the package to validate.
    /// </summary>
    public virtual string? PackageId { get; internal set; }

    /// <summary>
    ///     Gets the optional version of the package to validate.
    /// </summary>
    /// <remarks>
    ///     Defaults to the latest package version.
    /// </remarks>
    public virtual string? PackageVersion { get; internal set; }

    /// <summary>
    ///     Gets the directory from where the NuGet configuration file is loaded.
    /// </summary>
    /// <remarks>
    ///     Defaults to the current directory.
    /// </remarks>
    public virtual string? ConfigDirectory { get; internal set; }

    /// <inheritdoc />
    protected override Arguments ConfigureProcessArguments(Arguments arguments)
    {
        _ = arguments
            .Add("dotnet-validate package remote")
            .Add("{value}", PackageId)
            .Add("--version {value}", PackageVersion)
            .Add("--nuget-config-directory {value}", ConfigDirectory);

        return base.ConfigureProcessArguments(arguments);
    }
}
