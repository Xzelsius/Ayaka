// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.Nuke.Tests.DotNetValidate;

using Ayaka.Nuke.DotNetValidate;

public sealed class DotNetValidateRemotePackageSettingsExtensionsTest
{
    [Fact]
    public void Does_set_PackageId()
    {
        var original = new DotNetValidateRemotePackageSettings();

        var actual = original.SetPackageId("mypackage");

        actual.Should().NotBeSameAs(original);
        original.PackageId.Should().Be(expected: null);
        actual.PackageId.Should().Be("mypackage");
    }

    [Fact]
    public void Does_reset_PackageId()
    {
        var original = new DotNetValidateRemotePackageSettings()
            .SetPackageId("mypackage");

        var actual = original.ResetPackageId();

        actual.Should().NotBeSameAs(original);
        original.PackageId.Should().Be("mypackage");
        actual.PackageId.Should().Be(expected: null);
    }

    [Fact]
    public void Does_set_PackageVersion()
    {
        var original = new DotNetValidateRemotePackageSettings();

        var actual = original.SetPackageVersion("1.2.3.4");

        actual.Should().NotBeSameAs(original);
        original.PackageVersion.Should().Be(expected: null);
        actual.PackageVersion.Should().Be("1.2.3.4");
    }

    [Fact]
    public void Does_reset_PackageVersion()
    {
        var original = new DotNetValidateRemotePackageSettings()
            .SetPackageVersion("1.2.3.4");

        var actual = original.ResetPackageVersion();

        actual.Should().NotBeSameAs(original);
        original.PackageVersion.Should().Be("1.2.3.4");
        actual.PackageVersion.Should().Be(expected: null);
    }

    [Fact]
    public void Does_set_ConfigDirectory()
    {
        var original = new DotNetValidateRemotePackageSettings();

        var actual = original.SetConfigDirectory("path/to/directory");

        actual.Should().NotBeSameAs(original);
        original.ConfigDirectory.Should().Be(expected: null);
        actual.ConfigDirectory.Should().Be("path/to/directory");
    }

    [Fact]
    public void Does_reset_ConfigDirectory()
    {
        var original = new DotNetValidateRemotePackageSettings()
            .SetConfigDirectory("path/to/directory");

        var actual = original.ResetConfigDirectory();

        actual.Should().NotBeSameAs(original);
        original.ConfigDirectory.Should().Be("path/to/directory");
        actual.ConfigDirectory.Should().Be(expected: null);
    }
}
