// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.Nuke.Tests.GitHub;

using Ayaka.Nuke.GitHub;
using Light.GuardClauses.Exceptions;

public sealed class GitHubReleaseSettingsExtensionsTest
{
    [Fact]
    public void Does_set_Tag()
    {
        var original = new GitHubReleaseSettings();

        var actual = original.SetTag("v1.0");

        actual.Should().NotBeSameAs(original);
        original.Tag.Should().Be(expected: null);
        actual.Tag.Should().Be("v1.0");
    }

    [Fact]
    public void Does_reset_Tag()
    {
        var original = new GitHubReleaseSettings()
            .SetTag("v1.0");

        var actual = original.ResetTag();

        actual.Should().NotBeSameAs(original);
        original.Tag.Should().Be("v1.0");
        actual.Tag.Should().Be(expected: null);
    }

    [Fact]
    public void Does_set_TargetCommitish()
    {
        var original = new GitHubReleaseSettings();

        var actual = original.SetTargetCommitish("main");

        actual.Should().NotBeSameAs(original);
        original.TargetCommitish.Should().Be(expected: null);
        actual.TargetCommitish.Should().Be("main");
    }

    [Fact]
    public void Does_reset_TargetCommitish()
    {
        var original = new GitHubReleaseSettings()
            .SetTargetCommitish("main");

        var actual = original.ResetTargetCommitish();

        actual.Should().NotBeSameAs(original);
        original.TargetCommitish.Should().Be("main");
        actual.TargetCommitish.Should().Be(expected: null);
    }

    [Fact]
    public void Does_set_Name()
    {
        var original = new GitHubReleaseSettings();

        var actual = original.SetName("Release v1.0");

        actual.Should().NotBeSameAs(original);
        original.Name.Should().Be(expected: null);
        actual.Name.Should().Be("Release v1.0");
    }

    [Fact]
    public void Does_reset_Name()
    {
        var original = new GitHubReleaseSettings()
            .SetName("Release v1.0");

        var actual = original.ResetName();

        actual.Should().NotBeSameAs(original);
        original.Name.Should().Be("Release v1.0");
        actual.Name.Should().Be(expected: null);
    }

    [Fact]
    public void Does_set_Body()
    {
        var original = new GitHubReleaseSettings();

        var actual = original.SetBody("This is the release notes for v1.0.");

        actual.Should().NotBeSameAs(original);
        original.Body.Should().Be(expected: null);
        actual.Body.Should().Be("This is the release notes for v1.0.");
    }

    [Fact]
    public void Does_reset_Body()
    {
        var original = new GitHubReleaseSettings()
            .SetBody("This is the release notes for v1.0.");

        var actual = original.ResetBody();

        actual.Should().NotBeSameAs(original);
        original.Body.Should().Be("This is the release notes for v1.0.");
        actual.Body.Should().Be(expected: null);
    }

    [Fact]
    public void Does_set_Draft()
    {
        var original = new GitHubReleaseSettings();

        var actual = original.SetDraft(draft: true);

        actual.Should().NotBeSameAs(original);
        original.Draft.Should().Be(expected: null);
        actual.Draft.Should().Be(expected: true);
    }

    [Fact]
    public void Does_reset_Draft()
    {
        var original = new GitHubReleaseSettings()
            .SetDraft(draft: true);

        var actual = original.ResetDraft();

        actual.Should().NotBeSameAs(original);
        original.Draft.Should().Be(expected: true);
        actual.Draft.Should().Be(expected: null);
    }

    [Fact]
    public void Does_set_PreRelease()
    {
        var original = new GitHubReleaseSettings();

        var actual = original.SetPreRelease(preRelease: true);

        actual.Should().NotBeSameAs(original);
        original.PreRelease.Should().Be(expected: null);
        actual.PreRelease.Should().Be(expected: true);
    }

    [Fact]
    public void Does_reset_PreRelease()
    {
        var original = new GitHubReleaseSettings()
            .SetPreRelease(preRelease: true);

        var actual = original.ResetPreRelease();

        actual.Should().NotBeSameAs(original);
        original.PreRelease.Should().Be(expected: true);
        actual.PreRelease.Should().Be(expected: null);
    }

    [Fact]
    public void Does_set_GenerateReleaseNotes()
    {
        var original = new GitHubReleaseSettings();

        var actual = original.SetGenerateReleaseNotes(generateReleaseNotes: true);

        actual.Should().NotBeSameAs(original);
        original.GenerateReleaseNotes.Should().Be(expected: null);
        actual.GenerateReleaseNotes.Should().Be(expected: true);
    }

    [Fact]
    public void Does_reset_GenerateReleaseNotes()
    {
        var original = new GitHubReleaseSettings()
            .SetGenerateReleaseNotes(generateReleaseNotes: true);

        var actual = original.ResetGenerateReleaseNotes();

        actual.Should().NotBeSameAs(original);
        original.GenerateReleaseNotes.Should().Be(expected: true);
        actual.GenerateReleaseNotes.Should().Be(expected: null);
    }

    [Fact]
    public void Does_add_ArtifactPath()
    {
        var original = new GitHubReleaseSettings();

        var actual = original.AddArtifactPath("path/to/artifact");

        actual.Should().NotBeSameAs(original);
        original.ArtifactPaths.Should().BeNull();
        actual.ArtifactPaths.Should().ContainSingle().Which.Should().Be("path/to/artifact");
    }

    [Fact]
    public void Does_clear_ArtifactPath()
    {
        var original = new GitHubReleaseSettings()
            .AddArtifactPath("path/to/artifact");

        var actual = original.ClearArtifactPaths();

        actual.Should().NotBeSameAs(original);
        original.ArtifactPaths.Should().ContainSingle().Which.Should().Be("path/to/artifact");
        actual.ArtifactPaths.Should().BeEmpty();
    }
}
