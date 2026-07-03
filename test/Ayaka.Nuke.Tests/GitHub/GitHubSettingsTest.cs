// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.Nuke.Tests.GitHub;

using Ayaka.Nuke.GitHub;
using Light.GuardClauses.Exceptions;

public sealed class GitHubSettingsExtensionsTest
{
    [Fact]
    public void Does_set_RepositoryOwner()
    {
        var original = new GitHubSettings();

        var actual = original.SetRepositoryOwner("owner");

        actual.ShouldNotBeSameAs(original);
        original.RepositoryOwner.ShouldBeNull();
        actual.RepositoryOwner.ShouldBe("owner");
    }

    [Fact]
    public void Does_reset_RepositoryOwner()
    {
        var original = new GitHubSettings()
            .SetRepositoryOwner("owner");

        var actual = original.ResetRepositoryOwner();

        actual.ShouldNotBeSameAs(original);
        original.RepositoryOwner.ShouldBe("owner");
        actual.RepositoryOwner.ShouldBeNull();
    }

    [Fact]
    public void Does_set_RepositoryName()
    {
        var original = new GitHubSettings();

        var actual = original.SetRepositoryName("name");

        actual.ShouldNotBeSameAs(original);
        original.RepositoryName.ShouldBeNull();
        actual.RepositoryName.ShouldBe("name");
    }

    [Fact]
    public void Does_reset_RepositoryName()
    {
        var original = new GitHubSettings()
            .SetRepositoryName("name");

        var actual = original.ResetRepositoryName();

        actual.ShouldNotBeSameAs(original);
        original.RepositoryName.ShouldBe("name");
        actual.RepositoryName.ShouldBeNull();
    }

    [Fact]
    public void Does_set_Token()
    {
        var original = new GitHubSettings();

        var actual = original.SetToken("token");

        actual.ShouldNotBeSameAs(original);
        original.Token.ShouldBeNull();
        actual.Token.ShouldBe("token");
    }

    [Fact]
    public void Does_reset_Token()
    {
        var original = new GitHubSettings()
            .SetToken("token");

        var actual = original.ResetToken();

        actual.ShouldNotBeSameAs(original);
        original.Token.ShouldBe("token");
        actual.Token.ShouldBeNull();
    }

    [Fact]
    public void Does_set_BaseUrl()
    {
        var original = new GitHubSettings();

        var actual = original.SetBaseUrl("url");

        actual.ShouldNotBeSameAs(original);
        original.BaseUrl.ShouldBeNull();
        actual.BaseUrl.ShouldBe("url");
    }

    [Fact]
    public void Does_reset_BaseUrl()
    {
        var original = new GitHubSettings()
            .SetBaseUrl("url");

        var actual = original.ResetBaseUrl();

        actual.ShouldNotBeSameAs(original);
        original.BaseUrl.ShouldBe("url");
        actual.BaseUrl.ShouldBeNull();
    }
}
