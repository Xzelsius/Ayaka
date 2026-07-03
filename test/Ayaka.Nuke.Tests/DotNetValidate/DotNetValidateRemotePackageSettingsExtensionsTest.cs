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

        actual.ShouldNotBeSameAs(original);
        original.PackageId.ShouldBeNull();
        actual.PackageId.ShouldBe("mypackage");
    }

    [Fact]
    public void Does_reset_PackageId()
    {
        var original = new DotNetValidateRemotePackageSettings()
            .SetPackageId("mypackage");

        var actual = original.ResetPackageId();

        actual.ShouldNotBeSameAs(original);
        original.PackageId.ShouldBe("mypackage");
        actual.PackageId.ShouldBeNull();
    }

    [Fact]
    public void Does_set_PackageVersion()
    {
        var original = new DotNetValidateRemotePackageSettings();

        var actual = original.SetPackageVersion("1.2.3.4");

        actual.ShouldNotBeSameAs(original);
        original.PackageVersion.ShouldBeNull();
        actual.PackageVersion.ShouldBe("1.2.3.4");
    }

    [Fact]
    public void Does_reset_PackageVersion()
    {
        var original = new DotNetValidateRemotePackageSettings()
            .SetPackageVersion("1.2.3.4");

        var actual = original.ResetPackageVersion();

        actual.ShouldNotBeSameAs(original);
        original.PackageVersion.ShouldBe("1.2.3.4");
        actual.PackageVersion.ShouldBeNull();
    }

    [Fact]
    public void Does_set_ConfigDirectory()
    {
        var original = new DotNetValidateRemotePackageSettings();

        var actual = original.SetConfigDirectory("path/to/directory");

        actual.ShouldNotBeSameAs(original);
        original.ConfigDirectory.ShouldBeNull();
        actual.ConfigDirectory.ShouldBe("path/to/directory");
    }

    [Fact]
    public void Does_reset_ConfigDirectory()
    {
        var original = new DotNetValidateRemotePackageSettings()
            .SetConfigDirectory("path/to/directory");

        var actual = original.ResetConfigDirectory();

        actual.ShouldNotBeSameAs(original);
        original.ConfigDirectory.ShouldBe("path/to/directory");
        actual.ConfigDirectory.ShouldBeNull();
    }
}
