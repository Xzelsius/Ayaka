// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.Nuke.DotNetValidate;

using global::Nuke.Common.Tooling;

/// <summary>
///     Provides settings for validating a local NuGet package using <c>dotnet-validate</c> CLI tool.
/// </summary>
[Serializable]
public class DotNetValidateLocalPackageSettings : ToolSettings
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
    ///     Gets the path to the package to validate.
    /// </summary>
    public virtual string? PackagePath { get; internal set; }

    /// <inheritdoc />
    protected override Arguments ConfigureProcessArguments(Arguments arguments)
    {
        _ = arguments
            .Add("dotnet-validate package local")
            .Add("{value}", PackagePath);

        return base.ConfigureProcessArguments(arguments);
    }
}
