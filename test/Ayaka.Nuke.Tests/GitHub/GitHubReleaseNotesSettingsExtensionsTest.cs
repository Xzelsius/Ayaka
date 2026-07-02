// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.Nuke.Tests.GitHub;

using Ayaka.Nuke.GitHub;
using Light.GuardClauses.Exceptions;

public sealed class GitHubReleaseNotesSettingsExtensionsTest
{
    [Fact]
    public void Does_set_Tag()
    {
        var original = new GitHubReleaseNotesSettings();

        var actual = original.SetTag("v1.0");

        actual.ShouldNotBeSameAs(original);
        original.Tag.ShouldBeNull();
        actual.Tag.ShouldBe("v1.0");
    }

    [Fact]
    public void Does_reset_Tag()
    {
        var original = new GitHubReleaseNotesSettings()
            .SetTag("v1.0");

        var actual = original.ResetTag();

        actual.ShouldNotBeSameAs(original);
        original.Tag.ShouldBe("v1.0");
        actual.Tag.ShouldBeNull();
    }

    [Fact]
    public void Does_set_TargetCommitish()
    {
        var original = new GitHubReleaseNotesSettings();

        var actual = original.SetTargetCommitish("main");

        actual.ShouldNotBeSameAs(original);
        original.TargetCommitish.ShouldBeNull();
        actual.TargetCommitish.ShouldBe("main");
    }

    [Fact]
    public void Does_reset_TargetCommitish()
    {
        var original = new GitHubReleaseNotesSettings()
            .SetTargetCommitish("main");

        var actual = original.ResetTargetCommitish();

        actual.ShouldNotBeSameAs(original);
        original.TargetCommitish.ShouldBe("main");
        actual.TargetCommitish.ShouldBeNull();
    }

    [Fact]
    public void Does_set_PreviousTag()
    {
        var original = new GitHubReleaseNotesSettings();

        var actual = original.SetPreviousTag("v0.9");

        actual.ShouldNotBeSameAs(original);
        original.PreviousTag.ShouldBeNull();
        actual.PreviousTag.ShouldBe("v0.9");
    }

    [Fact]
    public void Does_reset_PreviousTag()
    {
        var original = new GitHubReleaseNotesSettings()
            .SetPreviousTag("v0.9");

        var actual = original.ResetPreviousTag();

        actual.ShouldNotBeSameAs(original);
        original.PreviousTag.ShouldBe("v0.9");
        actual.PreviousTag.ShouldBeNull();
    }

    [Fact]
    public void Does_set_ConfigFile()
    {
        var original = new GitHubReleaseNotesSettings();

        var actual = original.SetConfigFile("release.yml");

        actual.ShouldNotBeSameAs(original);
        original.ConfigFile.ShouldBeNull();
        actual.ConfigFile.ShouldBe("release.yml");
    }

    [Fact]
    public void Does_reset_ConfigFile()
    {
        var original = new GitHubReleaseNotesSettings()
            .SetConfigFile("release.yml");

        var actual = original.ResetConfigFile();

        actual.ShouldNotBeSameAs(original);
        original.ConfigFile.ShouldBe("release.yml");
        actual.ConfigFile.ShouldBeNull();
    }
}
