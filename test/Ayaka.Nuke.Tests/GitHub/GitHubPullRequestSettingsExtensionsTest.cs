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

        actual.Should().NotBeSameAs(original);
        original.Head.Should().Be(expected: null);
        actual.Head.Should().Be("feature/awesome");
    }

    [Fact]
    public void Does_reset_Head()
    {
        var original = new GitHubPullRequestSettings()
            .SetHead("feature/awesome");

        var actual = original.ResetHead();

        actual.Should().NotBeSameAs(original);
        original.Head.Should().Be("feature/awesome");
        actual.Head.Should().Be(expected: null);
    }

    [Fact]
    public void Does_set_Base()
    {
        var original = new GitHubPullRequestSettings();

        var actual = original.SetBase("main");

        actual.Should().NotBeSameAs(original);
        original.Base.Should().Be(expected: null);
        actual.Base.Should().Be("main");
    }

    [Fact]
    public void Does_reset_Base()
    {
        var original = new GitHubPullRequestSettings()
            .SetBase("main");

        var actual = original.ResetBase();

        actual.Should().NotBeSameAs(original);
        original.Base.Should().Be("main");
        actual.Base.Should().Be(expected: null);
    }

    [Fact]
    public void Does_set_Title()
    {
        var original = new GitHubPullRequestSettings();

        var actual = original.SetTitle("Add new feature");

        actual.Should().NotBeSameAs(original);
        original.Title.Should().Be(expected: null);
        actual.Title.Should().Be("Add new feature");
    }

    [Fact]
    public void Does_reset_Title()
    {
        var original = new GitHubPullRequestSettings()
            .SetTitle("Add new feature");

        var actual = original.ResetTitle();

        actual.Should().NotBeSameAs(original);
        original.Title.Should().Be("Add new feature");
        actual.Title.Should().Be(expected: null);
    }

    [Fact]
    public void Does_set_Body()
    {
        var original = new GitHubPullRequestSettings();

        var actual = original.SetBody("This pull request adds a new feature.");

        actual.Should().NotBeSameAs(original);
        original.Body.Should().Be(expected: null);
        actual.Body.Should().Be("This pull request adds a new feature.");
    }

    [Fact]
    public void Does_reset_Body()
    {
        var original = new GitHubPullRequestSettings()
            .SetBody("This pull request adds a new feature.");

        var actual = original.ResetBody();

        actual.Should().NotBeSameAs(original);
        original.Body.Should().Be("This pull request adds a new feature.");
        actual.Body.Should().Be(expected: null);
    }

    [Fact]
    public void Does_set_Draft()
    {
        var original = new GitHubPullRequestSettings();

        var actual = original.SetDraft(true);

        actual.Should().NotBeSameAs(original);
        original.Draft.Should().Be(expected: null);
        actual.Draft.Should().Be(true);
    }

    [Fact]
    public void Does_reset_Draft()
    {
        var original = new GitHubPullRequestSettings()
            .SetDraft(true);

        var actual = original.ResetDraft();

        actual.Should().NotBeSameAs(original);
        original.Draft.Should().Be(true);
        actual.Draft.Should().Be(expected: null);
    }
}
