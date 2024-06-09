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

        actual.Should().NotBeSameAs(original);
        original.Tag.Should().Be(expected: null);
        actual.Tag.Should().Be("v1.0");
    }

    [Fact]
    public void Does_reset_Tag()
    {
        var original = new GitHubReleaseNotesSettings()
            .SetTag("v1.0");

        var actual = original.ResetTag();

        actual.Should().NotBeSameAs(original);
        original.Tag.Should().Be("v1.0");
        actual.Tag.Should().Be(expected: null);
    }

    [Fact]
    public void Does_set_TargetCommitish()
    {
        var original = new GitHubReleaseNotesSettings();

        var actual = original.SetTargetCommitish("main");

        actual.Should().NotBeSameAs(original);
        original.TargetCommitish.Should().Be(expected: null);
        actual.TargetCommitish.Should().Be("main");
    }

    [Fact]
    public void Does_reset_TargetCommitish()
    {
        var original = new GitHubReleaseNotesSettings()
            .SetTargetCommitish("main");

        var actual = original.ResetTargetCommitish();

        actual.Should().NotBeSameAs(original);
        original.TargetCommitish.Should().Be("main");
        actual.TargetCommitish.Should().Be(expected: null);
    }

    [Fact]
    public void Does_set_PreviousTag()
    {
        var original = new GitHubReleaseNotesSettings();

        var actual = original.SetPreviousTag("v0.9");

        actual.Should().NotBeSameAs(original);
        original.PreviousTag.Should().Be(expected: null);
        actual.PreviousTag.Should().Be("v0.9");
    }

    [Fact]
    public void Does_reset_PreviousTag()
    {
        var original = new GitHubReleaseNotesSettings()
            .SetPreviousTag("v0.9");

        var actual = original.ResetPreviousTag();

        actual.Should().NotBeSameAs(original);
        original.PreviousTag.Should().Be("v0.9");
        actual.PreviousTag.Should().Be(expected: null);
    }

    [Fact]
    public void Does_set_ConfigFile()
    {
        var original = new GitHubReleaseNotesSettings();

        var actual = original.SetConfigFile("release.yml");

        actual.Should().NotBeSameAs(original);
        original.ConfigFile.Should().Be(expected: null);
        actual.ConfigFile.Should().Be("release.yml");
    }

    [Fact]
    public void Does_reset_ConfigFile()
    {
        var original = new GitHubReleaseNotesSettings()
            .SetConfigFile("release.yml");

        var actual = original.ResetConfigFile();

        actual.Should().NotBeSameAs(original);
        original.ConfigFile.Should().Be("release.yml");
        actual.ConfigFile.Should().Be(expected: null);
    }

    [Fact]
    public void Does_validate_settings_Tag_null()
    {
        var settings = new GitHubReleaseNotesSettings()
            .SetRepositoryOwner("owner")
            .SetRepositoryName("name")
            .SetToken("token")
            .SetTag(null!);

        var action = () => settings.Validate();

        action
            .Should()
            .Throw<ArgumentNullException>()
            .WithParameterName(nameof(GitHubReleaseNotesSettings.Tag));
    }

    [Fact]
    public void Does_validate_settings_Tag_empty()
    {
        var settings = new GitHubReleaseNotesSettings()
            .SetRepositoryOwner("owner")
            .SetRepositoryName("name")
            .SetToken("token")
            .SetTag("");

        var action = () => settings.Validate();

        action
            .Should()
            .Throw<EmptyStringException>()
            .WithParameterName(nameof(GitHubReleaseNotesSettings.Tag));
    }
}
