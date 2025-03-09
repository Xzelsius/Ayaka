// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.Nuke.DotNetValidate;

using global::Nuke.Common.Tooling;

/// <summary>
///     Extension methods for <see cref="DotNetValidateRemotePackageSettings" />.
/// </summary>
public static class DotNetValidateRemotePackageSettingsExtensions
{
    /// <summary>
    ///     Sets the identifier of the package to validate.
    /// </summary>
    /// <param name="toolSettings">The settings instance to adjust.</param>
    /// <param name="packageId">The identifier of the package to validate.</param>
    [Builder(
        Type = typeof(DotNetValidateRemotePackageSettings),
        Property = nameof(DotNetValidateRemotePackageSettings.PackageId))]
    public static T SetPackageId<T>(this T toolSettings, string packageId)
        where T : DotNetValidateRemotePackageSettings
        => toolSettings.Modify(
            x => x.Set(
                () => toolSettings.PackageId,
                packageId));

    /// <summary>
    ///     Resets the identifier of the package to validate to <c>null</c>.
    /// </summary>
    /// <param name="toolSettings">The settings instance to adjust.</param>
    [Builder(
        Type = typeof(DotNetValidateRemotePackageSettings),
        Property = nameof(DotNetValidateRemotePackageSettings.PackageId))]
    public static T ResetPackageId<T>(this T toolSettings)
        where T : DotNetValidateRemotePackageSettings
        => toolSettings.Modify(
            x => x.Remove(
                () => toolSettings.PackageId));

    /// <summary>
    ///     Sets the version of the package to validate.
    /// </summary>
    /// <param name="toolSettings">The settings instance to adjust.</param>
    /// <param name="packageVersion">The version of the package to validate.</param>
    [Builder(
        Type = typeof(DotNetValidateRemotePackageSettings),
        Property = nameof(DotNetValidateRemotePackageSettings.PackageVersion))]
    public static T SetPackageVersion<T>(this T toolSettings, string packageVersion)
        where T : DotNetValidateRemotePackageSettings
        => toolSettings.Modify(
            x => x.Set(
                () => toolSettings.PackageVersion,
                packageVersion));

    /// <summary>
    ///     Resets the version of the package to validate to <c>null</c>.
    /// </summary>
    /// <param name="toolSettings">The settings instance to adjust.</param>
    [Builder(
        Type = typeof(DotNetValidateRemotePackageSettings),
        Property = nameof(DotNetValidateRemotePackageSettings.PackageVersion))]
    public static T ResetPackageVersion<T>(this T toolSettings)
        where T : DotNetValidateRemotePackageSettings
        => toolSettings.Modify(
            x => x.Remove(
                () => toolSettings.PackageVersion));

    /// <summary>
    ///     Sets the directory from where the NuGet configuration file is loaded.
    /// </summary>
    /// <param name="toolSettings">The settings instance to adjust.</param>
    /// <param name="configFile">The directory from where the NuGet configuration file is loaded.</param>$
    [Builder(
        Type = typeof(DotNetValidateRemotePackageSettings),
        Property = nameof(DotNetValidateRemotePackageSettings.ConfigDirectory))]
    public static T SetConfigDirectory<T>(this T toolSettings, string configFile)
        where T : DotNetValidateRemotePackageSettings
        => toolSettings.Modify(
            x => x.Set(
                () => toolSettings.ConfigDirectory,
                configFile));

    /// <summary>
    ///     Resets the directory from where the NuGet configuration file is loaded to <c>null</c>.
    /// </summary>
    /// <param name="toolSettings">The settings instance to adjust.</param>
    [Builder(
        Type = typeof(DotNetValidateRemotePackageSettings),
        Property = nameof(DotNetValidateRemotePackageSettings.ConfigDirectory))]
    public static T ResetConfigDirectory<T>(this T toolSettings)
        where T : DotNetValidateRemotePackageSettings
        => toolSettings.Modify(
            x => x.Remove(
                () => toolSettings.ConfigDirectory));
}
