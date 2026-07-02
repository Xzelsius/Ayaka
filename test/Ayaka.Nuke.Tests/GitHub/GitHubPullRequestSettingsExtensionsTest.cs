// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.Nuke.Tests.GitHub;

using Ayaka.Nuke.GitHub;
using Light.GuardClauses.Exceptions;

public sealed class GitHubPullRequestSettingsExtensionsTest
{
    [Fact]
    public void Does_set_Head()
    {
        var original = new GitHubPullRequestSettings();

        var actual = original.SetHead("feature/awesome");

        actual.ShouldNotBeSameAs(original);
        original.Head.ShouldBeNull();
        actual.Head.ShouldBe("feature/awesome");
    }

    [Fact]
    public void Does_reset_Head()
    {
        var original = new GitHubPullRequestSettings()
            .SetHead("feature/awesome");

        var actual = original.ResetHead();

        actual.ShouldNotBeSameAs(original);
        original.Head.ShouldBe("feature/awesome");
        actual.Head.ShouldBeNull();
    }

    [Fact]
    public void Does_set_Base()
    {
        var original = new GitHubPullRequestSettings();

        var actual = original.SetBase("main");

        actual.ShouldNotBeSameAs(original);
        original.Base.ShouldBeNull();
        actual.Base.ShouldBe("main");
    }

    [Fact]
    public void Does_reset_Base()
    {
        var original = new GitHubPullRequestSettings()
            .SetBase("main");

        var actual = original.ResetBase();

        actual.ShouldNotBeSameAs(original);
        original.Base.ShouldBe("main");
        actual.Base.ShouldBeNull();
    }

    [Fact]
    public void Does_set_Title()
    {
        var original = new GitHubPullRequestSettings();

        var actual = original.SetTitle("Add new feature");

        actual.ShouldNotBeSameAs(original);
        original.Title.ShouldBeNull();
        actual.Title.ShouldBe("Add new feature");
    }

    [Fact]
    public void Does_reset_Title()
    {
        var original = new GitHubPullRequestSettings()
            .SetTitle("Add new feature");

        var actual = original.ResetTitle();

        actual.ShouldNotBeSameAs(original);
        original.Title.ShouldBe("Add new feature");
        actual.Title.ShouldBeNull();
    }

    [Fact]
    public void Does_set_Body()
    {
        var original = new GitHubPullRequestSettings();

        var actual = original.SetBody("This pull request adds a new feature.");

        actual.ShouldNotBeSameAs(original);
        original.Body.ShouldBeNull();
        actual.Body.ShouldBe("This pull request adds a new feature.");
    }

    [Fact]
    public void Does_reset_Body()
    {
        var original = new GitHubPullRequestSettings()
            .SetBody("This pull request adds a new feature.");

        var actual = original.ResetBody();

        actual.ShouldNotBeSameAs(original);
        original.Body.ShouldBe("This pull request adds a new feature.");
        actual.Body.ShouldBeNull();
    }

    [Fact]
    public void Does_set_Draft()
    {
        var original = new GitHubPullRequestSettings();

        var actual = original.SetDraft(true);

        actual.ShouldNotBeSameAs(original);
        original.Draft.ShouldBeNull();
        actual.Draft.ShouldBe(true);
    }

    [Fact]
    public void Does_reset_Draft()
    {
        var original = new GitHubPullRequestSettings()
            .SetDraft(true);

        var actual = original.ResetDraft();

        actual.ShouldNotBeSameAs(original);
        original.Draft.ShouldBe(true);
        actual.Draft.ShouldBeNull();
    }
}
