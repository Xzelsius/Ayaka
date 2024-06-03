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
    public static T SetPackageId<T>(this T toolSettings, string packageId)
        where T : DotNetValidateRemotePackageSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.PackageId = packageId;
        return toolSettings;
    }

    /// <summary>
    ///     Resets the identifier of the package to validate to <c>null</c>.
    /// </summary>
    /// <param name="toolSettings">The settings instance to adjust.</param>
    public static T ResetPackageId<T>(this T toolSettings)
        where T : DotNetValidateRemotePackageSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.PackageId = null;
        return toolSettings;
    }

    /// <summary>
    ///     Sets the version of the package to validate.
    /// </summary>
    /// <param name="toolSettings">The settings instance to adjust.</param>
    /// <param name="packageVersion">The version of the package to validate.</param>
    public static T SetPackageVersion<T>(this T toolSettings, string packageVersion)
        where T : DotNetValidateRemotePackageSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.PackageVersion = packageVersion;
        return toolSettings;
    }

    /// <summary>
    ///     Resets the version of the package to validate to <c>null</c>.
    /// </summary>
    /// <param name="toolSettings">The settings instance to adjust.</param>
    public static T ResetPackageVersion<T>(this T toolSettings)
        where T : DotNetValidateRemotePackageSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.PackageVersion = null;
        return toolSettings;
    }

    /// <summary>
    ///     Sets the directory from where the NuGet configuration file is loaded.
    /// </summary>
    /// <param name="toolSettings">The settings instance to adjust.</param>
    /// <param name="configFile">The directory from where the NuGet configuration file is loaded.</param>
    public static T SetConfigDirectory<T>(this T toolSettings, string configFile)
        where T : DotNetValidateRemotePackageSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.ConfigDirectory = configFile;
        return toolSettings;
    }

    /// <summary>
    ///     Resets the directory from where the NuGet configuration file is loaded to <c>null</c>.
    /// </summary>
    /// <param name="toolSettings">The settings instance to adjust.</param>
    public static T ResetConfigDirectory<T>(this T toolSettings)
        where T : DotNetValidateRemotePackageSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.ConfigDirectory = null;
        return toolSettings;
    }
}
