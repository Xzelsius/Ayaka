// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.Nuke.Tests.DotNetValidate;

using Ayaka.Nuke.DotNetValidate;

public sealed class DotNetValidateRemotePackageSettingsTest
{
    [Theory]
    [InlineData(null, null, null, "dotnet-validate package remote")]
    [InlineData("mypackage", null, null, "dotnet-validate package remote mypackage")]
    [InlineData("mypackage", "1.2.3.4", null, "dotnet-validate package remote mypackage --version 1.2.3.4")]
    [InlineData(
        "mypackage",
        "1.2.3.4",
        "path/to/directory",
        "dotnet-validate package remote mypackage --version 1.2.3.4 --nuget-config-directory path/to/directory")]
    public void Does_configure_process_arguments(
        string? packageId,
        string? packageVersion,
        string? configDirectory,
        string expectedArguments)
    {
        var settings = new DotNetValidateRemotePackageSettings()
            .SetPackageId(packageId!)
            .SetPackageVersion(packageVersion!)
            .SetConfigDirectory(configDirectory!);

        var arguments = settings.GetProcessArguments();

        arguments
            .RenderForExecution()
            .Should()
            .BeEquivalentTo(expectedArguments);
    }
}
