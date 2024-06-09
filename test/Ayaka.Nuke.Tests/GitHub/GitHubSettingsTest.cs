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

        actual.Should().NotBeSameAs(original);
        original.RepositoryOwner.Should().Be(expected: null);
        actual.RepositoryOwner.Should().Be("owner");
    }

    [Fact]
    public void Does_reset_RepositoryOwner()
    {
        var original = new GitHubSettings()
            .SetRepositoryOwner("owner");

        var actual = original.ResetRepositoryOwner();

        actual.Should().NotBeSameAs(original);
        original.RepositoryOwner.Should().Be("owner");
        actual.RepositoryOwner.Should().Be(expected: null);
    }

    [Fact]
    public void Does_set_RepositoryName()
    {
        var original = new GitHubSettings();

        var actual = original.SetRepositoryName("name");

        actual.Should().NotBeSameAs(original);
        original.RepositoryName.Should().Be(expected: null);
        actual.RepositoryName.Should().Be("name");
    }

    [Fact]
    public void Does_reset_RepositoryName()
    {
        var original = new GitHubSettings()
            .SetRepositoryName("name");

        var actual = original.ResetRepositoryName();

        actual.Should().NotBeSameAs(original);
        original.RepositoryName.Should().Be("name");
        actual.RepositoryName.Should().Be(expected: null);
    }

    [Fact]
    public void Does_set_Token()
    {
        var original = new GitHubSettings();

        var actual = original.SetToken("token");

        actual.Should().NotBeSameAs(original);
        original.Token.Should().Be(expected: null);
        actual.Token.Should().Be("token");
    }

    [Fact]
    public void Does_reset_Token()
    {
        var original = new GitHubSettings()
            .SetToken("token");

        var actual = original.ResetToken();

        actual.Should().NotBeSameAs(original);
        original.Token.Should().Be("token");
        actual.Token.Should().Be(expected: null);
    }

    [Fact]
    public void Does_set_BaseUrl()
    {
        var original = new GitHubSettings();

        var actual = original.SetBaseUrl("url");

        actual.Should().NotBeSameAs(original);
        original.BaseUrl.Should().Be(expected: null);
        actual.BaseUrl.Should().Be("url");
    }

    [Fact]
    public void Does_reset_BaseUrl()
    {
        var original = new GitHubSettings()
            .SetBaseUrl("url");

        var actual = original.ResetBaseUrl();

        actual.Should().NotBeSameAs(original);
        original.BaseUrl.Should().Be("url");
        actual.BaseUrl.Should().Be(expected: null);
    }

    [Fact]
    public void Does_validate_settings_RepositoryOwner_null()
    {
        var settings = new GitHubSettings()
            .SetRepositoryOwner(null!);

        var action = () => settings.Validate();

        action
            .Should()
            .Throw<ArgumentNullException>()
            .WithParameterName(nameof(GitHubSettings.RepositoryOwner));
    }

    [Fact]
    public void Does_validate_settings_RepositoryOwner_empty()
    {
        var settings = new GitHubSettings()
            .SetRepositoryOwner("");

        var action = () => settings.Validate();

        action
            .Should()
            .Throw<EmptyStringException>()
            .WithParameterName(nameof(GitHubSettings.RepositoryOwner));
    }

    [Fact]
    public void Does_validate_settings_RepositoryName_null()
    {
        var settings = new GitHubSettings()
            .SetRepositoryOwner("owner")
            .SetRepositoryName(null!);

        var action = () => settings.Validate();

        action
            .Should()
            .Throw<ArgumentNullException>()
            .WithParameterName(nameof(GitHubSettings.RepositoryName));
    }

    [Fact]
    public void Does_validate_settings_RepositoryName_empty()
    {
        var settings = new GitHubSettings()
            .SetRepositoryOwner("owner")
            .SetRepositoryName("");

        var action = () => settings.Validate();

        action
            .Should()
            .Throw<EmptyStringException>()
            .WithParameterName(nameof(GitHubSettings.RepositoryName));
    }

    [Fact]
    public void Does_validate_settings_Token_null()
    {
        var settings = new GitHubSettings()
            .SetRepositoryOwner("owner")
            .SetRepositoryName("name")
            .SetToken(null!);

        var action = () => settings.Validate();

        action
            .Should()
            .Throw<ArgumentNullException>()
            .WithParameterName(nameof(GitHubSettings.Token));
    }

    [Fact]
    public void Does_validate_settings_Token_empty()
    {
        var settings = new GitHubSettings()
            .SetRepositoryOwner("owner")
            .SetRepositoryName("name")
            .SetToken("");

        var action = () => settings.Validate();

        action
            .Should()
            .Throw<EmptyStringException>()
            .WithParameterName(nameof(GitHubSettings.Token));
    }
}
