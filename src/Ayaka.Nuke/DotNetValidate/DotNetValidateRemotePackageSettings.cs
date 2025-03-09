// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.Nuke.DotNetValidate;

using global::Nuke.Common.Tooling;

/// <summary>
///     Provides settings for validating a remote NuGet package using <c>dotnet-validate</c> CLI tool.
/// </summary>
[Serializable]
[Command(
    Type = typeof(DotNetValidateTasks),
    Command = nameof(DotNetValidateTasks.DotNetValidateRemotePackage),
    Arguments = "package remote")]
public class DotNetValidateRemotePackageSettings : ToolOptions
{
    /// <summary>
    ///     Gets the identifier of the package to validate.
    /// </summary>
    [Argument(Format = "{value}", Position = 1)]
    public string PackageId => Get<string>(() => PackageId);

    /// <summary>
    ///     Gets the optional version of the package to validate.
    /// </summary>
    /// <remarks>
    ///     Defaults to the latest package version.
    /// </remarks>
    [Argument(Format = "--version {value}")]
    public string? PackageVersion => Get<string?>(() => PackageVersion);

    /// <summary>
    ///     Gets the directory from where the NuGet configuration file is loaded.
    /// </summary>
    /// <remarks>
    ///     Defaults to the current directory.
    /// </remarks>
    [Argument(Format = "--nuget-config-directory {value}")]
    public string? ConfigDirectory => Get<string?>(() => ConfigDirectory);
}
