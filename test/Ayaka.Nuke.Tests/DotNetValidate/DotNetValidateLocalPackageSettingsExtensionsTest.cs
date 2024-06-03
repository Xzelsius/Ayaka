// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.Nuke.Tests.DotNetValidate;

using Ayaka.Nuke.DotNetValidate;

public sealed class DotNetValidateLocalPackageSettingsExtensionsTest
{
    [Fact]
    public void Does_set_PackagePath()
    {
        var original = new DotNetValidateLocalPackageSettings();

        var actual = original.SetPackagePath("path/to/package");

        actual.Should().NotBeSameAs(original);
        original.PackagePath.Should().Be(expected: null);
        actual.PackagePath.Should().Be("path/to/package");
    }

    [Fact]
    public void Does_reset_PackagePath()
    {
        var original = new DotNetValidateLocalPackageSettings()
            .SetPackagePath("path/to/package");

        var actual = original.ResetPackagePath();

        actual.Should().NotBeSameAs(original);
        original.PackagePath.Should().Be("path/to/package");
        actual.PackagePath.Should().Be(expected: null);
    }
}
