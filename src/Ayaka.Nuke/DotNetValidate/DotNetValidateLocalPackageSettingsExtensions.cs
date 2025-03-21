// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.Nuke.DotNetValidate;

using global::Nuke.Common.Tooling;

/// <summary>
///     Extension methods for <see cref="DotNetValidateLocalPackageSettings" />.
/// </summary>
public static class DotNetValidateLocalPackageSettingsExtensions
{
    /// <summary>
    ///     Sets the path to the package to validate.
    /// </summary>
    /// <param name="toolSettings">The settings instance to adjust.</param>
    /// <param name="packagePath">The path to the package to validate.</param>
    [Builder(
        Type = typeof(DotNetValidateLocalPackageSettings),
        Property = nameof(DotNetValidateLocalPackageSettings.PackagePath))]
    public static T SetPackagePath<T>(this T toolSettings, string packagePath)
        where T : DotNetValidateLocalPackageSettings
        => toolSettings.Modify(
            x => x.Set(
                () => toolSettings.PackagePath,
                packagePath));

    /// <summary>
    ///     Resets the path of the package to validate to <c>null</c>.
    /// </summary>
    /// <param name="toolSettings">The settings instance to adjust.</param>
    [Builder(
        Type = typeof(DotNetValidateLocalPackageSettings),
        Property = nameof(DotNetValidateLocalPackageSettings.PackagePath))]
    public static T ResetPackagePath<T>(this T toolSettings)
        where T : DotNetValidateLocalPackageSettings
        => toolSettings.Modify(
            x => x.Remove(
                () => toolSettings.PackagePath));
}
