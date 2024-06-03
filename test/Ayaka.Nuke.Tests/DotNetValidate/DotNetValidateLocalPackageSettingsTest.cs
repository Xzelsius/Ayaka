// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.Nuke.Tests.DotNetValidate;

using Ayaka.Nuke.DotNetValidate;

public sealed class DotNetValidateLocalPackageSettingsTest
{
    [Theory]
    [InlineData(null, "dotnet-validate package local")]
    [InlineData("path/to/package", "dotnet-validate package local path/to/package")]
    public void Does_configure_process_arguments(string? packagePath, string expectedArguments)
    {
        var settings = new DotNetValidateLocalPackageSettings()
            .SetPackagePath(packagePath!);

        var arguments = settings.GetProcessArguments();

        arguments
            .RenderForExecution()
            .Should()
            .BeEquivalentTo(expectedArguments);
    }
}
