// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.Nuke.DotNetValidate;

using global::Nuke.Common;
using global::Nuke.Common.IO;
using global::Nuke.Common.Tooling;

/// <summary>
///     Provides a target for validating NuGet packages using <c>dotnet-validate</c> CLI tool to the
///     <see cref="INukeBuild" />.
/// </summary>
public interface ICanDotNetValidate
    : ICan,
        IHavePackageArtifacts,
        IHaveDotNetValidateTarget
{
    /// <summary>
    ///     Gets the base settings for validating a specific NuGet package.
    /// </summary>
    [ExcludeFromCodeCoverage]
    sealed Configure<DotNetValidateLocalPackageSettings, AbsolutePath> DotNetValidatePackageSettingsBase
        => (dotnetValidate, packagePath) => dotnetValidate
            .SetPackagePath(packagePath);

    /// <summary>
    ///     Gets the additional settings for validating a specific NuGet package.
    /// </summary>
    /// <remarks>
    ///     Override this to provide additional settings for the <see cref="IHaveDotNetValidateTarget.DotNetValidate" />
    ///     target.
    /// </remarks>
    [ExcludeFromCodeCoverage]
    Configure<DotNetValidateLocalPackageSettings, AbsolutePath> DotNetValidatePackageSettings
        => (dotnetValidate, packagePath) => dotnetValidate;

    /// <summary>
    ///     Gets all NuGet packages to validate.
    /// </summary>
    /// <remarks>
    ///     If not overridden, all <c>*.nupkg</c> files in the <see cref="IHavePackageArtifacts.PackagesDirectory" /> are
    ///     considered packages.
    /// </remarks>
    [ExcludeFromCodeCoverage]
    IEnumerable<AbsolutePath> NuGetPackagesToValidate => PackagesDirectory.GlobFiles("*.nupkg");

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    Target IHaveDotNetValidateTarget.DotNetValidate => target => target
        .Description("Validates all NuGet packages using 'dotnet-validate' CLI tool")
        .Unlisted()
        .OnlyWhenStatic(() => IsServerBuild)
        .Executes(() =>
        {
            _ = DotNetValidateTasks.DotNetValidateLocalPackage(
                dotnetValidate => dotnetValidate
                    .CombineWith(NuGetPackagesToValidate, (d, f) => d
                        .Apply(DotNetValidatePackageSettingsBase, f)
                        .Apply(DotNetValidatePackageSettings, f)),
                degreeOfParallelism: 1,
                completeOnFailure: true);
        });
}
