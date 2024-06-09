// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.Nuke.GitHub;

using System.Reflection;
using global::Nuke.Common.Tooling;
using Octokit;
using Serilog;

/// <summary>
///     Provides tasks for interacting with the GitHub API.
/// </summary>
public static class GitHubTasks
{
    /// <summary>
    ///     Creates a new GitHub release using the GitHub API.
    /// </summary>
    /// <param name="settings">The settings to for creating the release.</param>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    public static async Task CreateRelease(GitHubReleaseSettings settings)
    {
        settings.Validate();

        var client = CreateGitHubClient(settings.Token!, settings.BaseUrl);

        Log.Debug("Checking whether release with tag '{TagName}' already exists...", settings.Tag);
        
        var existingReleases = await client.Repository.Release.GetAll(
            settings.RepositoryOwner!,
            settings.RepositoryName!);
        
        if (existingReleases.Any(x => x.TagName == settings.Tag))
        {
            Log.Warning("Release with tag '{TagName}' already exists. No GitHub release is created", settings.Tag);
            return;
        }

        Log.Debug("Release with tag '{TagName}' not found", settings.Tag);

        var newRelease = new NewRelease(settings.Tag)
        {
            TargetCommitish = settings.TargetCommitish,
            Name = settings.Name,
            Body = settings.Body,
            Draft = settings.Draft ?? false,
            Prerelease = settings.PreRelease ?? false,
            GenerateReleaseNotes = settings.GenerateReleaseNotes ?? false
        };

        Log.Information("Creating release with tag '{TagName}'...", settings.Tag);
        Log.Verbose("  Target commitish: {TargetCommitish}", newRelease.TargetCommitish);
        Log.Verbose("  Name: {Name}", newRelease.Name);
        Log.Verbose("  Body: {Body}", newRelease.Body);
        Log.Verbose("  Draft: {Draft}", newRelease.Draft);
        Log.Verbose("  Pre-release: {PreRelease}", newRelease.Prerelease);
        Log.Verbose("  Generate release notes: {GenerateReleaseNotes}", newRelease.GenerateReleaseNotes);

        var release = await client.Repository.Release.Create(
            settings.RepositoryOwner!,
            settings.RepositoryName!,
            newRelease);

        Log.Information("Release with name '{Name}' created", release.Name);
        Log.Verbose("  Tag: {TagName}", release.TagName);
        Log.Verbose("  Target commitish: {TargetCommitish}", release.TargetCommitish);
        Log.Verbose("  Body: {Body}", release.Body);
        Log.Verbose("  Draft: {Draft}", release.Draft);
        Log.Verbose("  Pre-release: {PreRelease}", release.Prerelease);

        if (settings.ArtifactPaths.Count > 0)
        {
            Log.Debug("Uploading {ArtifactCount} artifact(s) to release...", settings.ArtifactPaths.Count);

            foreach (var artifactPath in settings.ArtifactPaths)
            {
                Log.Verbose("  Uploading artifact '{ArtifactPath}'...", artifactPath);

                await using var assetFile = File.OpenRead(artifactPath);

                var asset = new ReleaseAssetUpload
                {
                    FileName = Path.GetFileName(artifactPath),
                    ContentType = "application/octet-stream",
                    RawData = assetFile
                };

                var uploadedAsset = await client.Repository.Release.UploadAsset(release, asset);

                Log.Information("  Artifact '{ArtifactName}' uploaded", uploadedAsset.Name);
            }

            Log.Debug("All artifacts uploaded");
        }
    }

    /// <summary>
    ///     Creates a new GitHub release using the GitHub API.
    /// </summary>
    /// <param name="configure">A method to use for configuring the settings of the release.</param>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    public static Task CreateRelease(Configure<GitHubReleaseSettings> configure)
        => CreateRelease(configure(new GitHubReleaseSettings()));

    /// <summary>
    ///     Generates release notes for a specific tag using the GitHub API.
    /// </summary>
    /// <param name="settings">The settings to use for generating release notes.</param>
    /// <returns>
    ///     A <see cref="Task" /> representing the asynchronous operation. The task result contains the generated release
    ///     name and content.
    /// </returns>
    public static async Task<(string Name, string Body)> GenerateReleaseNotes(GitHubReleaseNotesSettings settings)
    {
        settings.Validate();

        var client = CreateGitHubClient(settings.Token!, settings.BaseUrl);

        Log.Information("Generating release notes for tag '{TagName}'...", settings.Tag);

        var releaseNotes = await client.Repository.Release.GenerateReleaseNotes(
            settings.RepositoryOwner!,
            settings.RepositoryName!,
            new GenerateReleaseNotesRequest(settings.Tag)
            {
                TargetCommitish = settings.TargetCommitish,
                PreviousTagName = settings.PreviousTag,
                ConfigurationFilePath = settings.ConfigFile
            });

        Log.Debug("Release notes for tag '{TagName}' generated", settings.Tag);
        Log.Verbose("  Name: {Name}", releaseNotes.Name);
        Log.Verbose("  Body: {Body}", releaseNotes.Body);

        return (releaseNotes.Name, releaseNotes.Body);
    }

    /// <summary>
    ///     Generates release notes for a specific tag using the GitHub API.
    /// </summary>
    /// <param name="configure">A method to use for configuring the settings of the generation.</param>
    /// <returns>
    ///     A <see cref="Task" /> representing the asynchronous operation. The task result contains the generated release
    ///     name and content.
    /// </returns>
    public static Task<(string Name, string Body)> GenerateReleaseNotes(Configure<GitHubReleaseNotesSettings> configure)
        => GenerateReleaseNotes(configure(new GitHubReleaseNotesSettings()));

    private static GitHubClient CreateGitHubClient(string token, string? baseAddress)
    {
        var productInformation = new ProductHeaderValue(Assembly.GetExecutingAssembly().GetName().Name);
        var credentials = new Credentials(token);

        return string.IsNullOrEmpty(baseAddress)
            ? new GitHubClient(productInformation) { Credentials = credentials }
            : new GitHubClient(productInformation, new Uri(baseAddress)) { Credentials = credentials };
    }
}
