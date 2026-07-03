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

        actual.ShouldNotBeSameAs(original);
        original.Tag.ShouldBeNull();
        actual.Tag.ShouldBe("v1.0");
    }

    [Fact]
    public void Does_reset_Tag()
    {
        var original = new GitHubReleaseSettings()
            .SetTag("v1.0");

        var actual = original.ResetTag();

        actual.ShouldNotBeSameAs(original);
        original.Tag.ShouldBe("v1.0");
        actual.Tag.ShouldBeNull();
    }

    [Fact]
    public void Does_set_TargetCommitish()
    {
        var original = new GitHubReleaseSettings();

        var actual = original.SetTargetCommitish("main");

        actual.ShouldNotBeSameAs(original);
        original.TargetCommitish.ShouldBeNull();
        actual.TargetCommitish.ShouldBe("main");
    }

    [Fact]
    public void Does_reset_TargetCommitish()
    {
        var original = new GitHubReleaseSettings()
            .SetTargetCommitish("main");

        var actual = original.ResetTargetCommitish();

        actual.ShouldNotBeSameAs(original);
        original.TargetCommitish.ShouldBe("main");
        actual.TargetCommitish.ShouldBeNull();
    }

    [Fact]
    public void Does_set_Name()
    {
        var original = new GitHubReleaseSettings();

        var actual = original.SetName("Release v1.0");

        actual.ShouldNotBeSameAs(original);
        original.Name.ShouldBeNull();
        actual.Name.ShouldBe("Release v1.0");
    }

    [Fact]
    public void Does_reset_Name()
    {
        var original = new GitHubReleaseSettings()
            .SetName("Release v1.0");

        var actual = original.ResetName();

        actual.ShouldNotBeSameAs(original);
        original.Name.ShouldBe("Release v1.0");
        actual.Name.ShouldBeNull();
    }

    [Fact]
    public void Does_set_Body()
    {
        var original = new GitHubReleaseSettings();

        var actual = original.SetBody("This is the release notes for v1.0.");

        actual.ShouldNotBeSameAs(original);
        original.Body.ShouldBeNull();
        actual.Body.ShouldBe("This is the release notes for v1.0.");
    }

    [Fact]
    public void Does_reset_Body()
    {
        var original = new GitHubReleaseSettings()
            .SetBody("This is the release notes for v1.0.");

        var actual = original.ResetBody();

        actual.ShouldNotBeSameAs(original);
        original.Body.ShouldBe("This is the release notes for v1.0.");
        actual.Body.ShouldBeNull();
    }

    [Fact]
    public void Does_set_Draft()
    {
        var original = new GitHubReleaseSettings();

        var actual = original.SetDraft(draft: true);

        actual.ShouldNotBeSameAs(original);
        original.Draft.ShouldBeNull();
        actual.Draft.ShouldBe(true);
    }

    [Fact]
    public void Does_reset_Draft()
    {
        var original = new GitHubReleaseSettings()
            .SetDraft(draft: true);

        var actual = original.ResetDraft();

        actual.ShouldNotBeSameAs(original);
        original.Draft.ShouldBe(true);
        actual.Draft.ShouldBeNull();
    }

    [Fact]
    public void Does_set_PreRelease()
    {
        var original = new GitHubReleaseSettings();

        var actual = original.SetPreRelease(preRelease: true);

        actual.ShouldNotBeSameAs(original);
        original.PreRelease.ShouldBeNull();
        actual.PreRelease.ShouldBe(true);
    }

    [Fact]
    public void Does_reset_PreRelease()
    {
        var original = new GitHubReleaseSettings()
            .SetPreRelease(preRelease: true);

        var actual = original.ResetPreRelease();

        actual.ShouldNotBeSameAs(original);
        original.PreRelease.ShouldBe(true);
        actual.PreRelease.ShouldBeNull();
    }

    [Fact]
    public void Does_set_GenerateReleaseNotes()
    {
        var original = new GitHubReleaseSettings();

        var actual = original.SetGenerateReleaseNotes(generateReleaseNotes: true);

        actual.ShouldNotBeSameAs(original);
        original.GenerateReleaseNotes.ShouldBeNull();
        actual.GenerateReleaseNotes.ShouldBe(true);
    }

    [Fact]
    public void Does_reset_GenerateReleaseNotes()
    {
        var original = new GitHubReleaseSettings()
            .SetGenerateReleaseNotes(generateReleaseNotes: true);

        var actual = original.ResetGenerateReleaseNotes();

        actual.ShouldNotBeSameAs(original);
        original.GenerateReleaseNotes.ShouldBe(true);
        actual.GenerateReleaseNotes.ShouldBeNull();
    }

    [Fact]
    public void Does_add_ArtifactPath()
    {
        var original = new GitHubReleaseSettings();

        var actual = original.AddArtifactPath("path/to/artifact");

        actual.ShouldNotBeSameAs(original);
        original.ArtifactPaths.ShouldBeNull();
        actual.ArtifactPaths.ShouldHaveSingleItem().ShouldBe("path/to/artifact");
    }

    [Fact]
    public void Does_clear_ArtifactPath()
    {
        var original = new GitHubReleaseSettings()
            .AddArtifactPath("path/to/artifact");

        var actual = original.ClearArtifactPaths();

        actual.ShouldNotBeSameAs(original);
        original.ArtifactPaths.ShouldHaveSingleItem().ShouldBe("path/to/artifact");
        actual.ArtifactPaths.ShouldBeEmpty();
    }
}
