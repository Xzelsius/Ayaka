// Copyright (c) Raphael Strotz. All rights reserved.

using Ayaka.Nuke;
using Ayaka.Nuke.DotNet;
using Ayaka.Nuke.DotNetValidate;
using Ayaka.Nuke.GitHub;
using Ayaka.Nuke.NuGet;
using Ayaka.Nuke.PublicApi;
using Ayaka.Nuke.VitePress;
using Nuke.Common;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.Npm;

[DotNetVerbosityMapping]
partial class Build
    : NukeBuild,
        IHaveGitRepository,
        IHaveGitVersion,
        IHaveSolution,
        IHaveSources,
        IHaveTests,
        IHaveDocumentation,
        IHaveArtifacts,
        IHaveTestArtifacts,
        IHaveCodeCoverage,
        IHavePackageArtifacts,
        IHaveDocumentationArtifacts,
        IHaveDotNetConfiguration,
        IHaveNuGetConfiguration,
        IHaveGitHubToken,
        ICanClean,
        ICanDotNetRestore,
        ICanDotNetBuild,
        ICanDotNetTest,
        ICanDotNetPack,
        ICanDotNetValidate,
        ICanDotNetPush,
        ICanVitePressInstall,
        ICanVitePressLint,
        ICanVitePressBuild,
        ICanGitHubRelease,
        ICanShipPublicApis
{
    public static int Main() => Execute<Build>(x => x.Default);

    Target Default => target => target
        .Description("Compiles, Tests and Packs everything in the Solution")
        .DependsOn(Compile)
        .DependsOn(Test)
        .DependsOn(Pack);

    Target Compile => target => target
        .Description("Builds all Projects in the Solution")
        .DependsOn<IHaveCleanTarget>()
        .DependsOn<IHaveDotNetBuildTarget>();

    Target Test => target => target
        .Description("Runs all Tests in the Solution")
        .DependsOn<IHaveCleanTarget>()
        .DependsOn<IHaveDotNetTestTarget>();

    Configure<DotNetPackSettings> ICanDotNetPack.DotNetPackSettings
        => settings => settings
            .WhenNotNull(
                this as IHaveGitVersion,
                (d, o) =>
                {
                    // If GitVersion evaluates a pre-release label, we use --version
                    // which will prevent any VersionSuffix set somewhere to be ignored
                    // See https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-pack
                    if (!string.IsNullOrEmpty(o.Versioning.PreReleaseLabel))
                    {
                        return d.SetVersion(o.Versioning.SemVer);
                    }

                    // But if we have no pre-release label (e.g. when the current commit is tagged)
                    // We reset any previously set --version and use --version-prefix,
                    // thus allowing individual packages to use a preview-label
                    return d.ResetVersion().SetVersionPrefix(o.Versioning.SemVer);
                });

    Target Pack => target => target
        .Description("Packs all NuGet packages in the Solution")
        .DependsOn<IHaveCleanTarget>()
        .DependsOn<IHaveDotNetPackTarget>()
        .DependsOn<IHaveDotNetValidateTarget>();

    Target Publish => target => target
        .Description("Publishes all NuGet packages in the Solution")
        .DependsOn<IHaveDotNetPushTarget>();

    // Customize the VitePress linting
    Configure<NpmRunSettings> ICanVitePressLint.VitePressLintSettings
        => run => run
            .SetCommand("lint:js");

    Target Docs => target => target
        .Description("Builds the Documentation")
        .DependsOn<IHaveVitePressBuildTarget>();

    Target CreateRelease => target => target
        .Description("Creates a new GitHub Release based on the current commit")
        .DependsOn<IHaveGitHubReleaseTarget>();

    Target AfterRelease => target => target
        .Description("Performs after-release steps and create a pull-request")
        .DependsOn(CreateAfterReleasePullRequest);
}
