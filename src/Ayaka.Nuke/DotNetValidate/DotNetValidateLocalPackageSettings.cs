// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.Nuke.DotNetValidate;

using global::Nuke.Common.Tooling;

/// <summary>
///     Provides settings for validating a local NuGet package using <c>dotnet-validate</c> CLI tool.
/// </summary>
[Serializable]
[Command(
    Type = typeof(DotNetValidateTasks),
    Command = nameof(DotNetValidateTasks.DotNetValidateLocalPackage),
    Arguments = "package local")]
public class DotNetValidateLocalPackageSettings : ToolOptions
{
    /// <summary>
    ///     Gets the path to the package to validate.
    /// </summary>
    [Argument(Format = "{value}", Position = 1)]
    public string PackagePath => Get<string>(() => PackagePath);
}
