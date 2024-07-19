// Copyright (c) Raphael Strotz. All rights reserved.

using Ayaka.Nuke;
using Ayaka.Nuke.DotNet;
using Ayaka.Nuke.DotNetValidate;
using Ayaka.Nuke.GitHub;
using Ayaka.Nuke.NuGet;
using Ayaka.Nuke.PublicApi;
using Ayaka.Nuke.VitePress;
using Nuke.Common;
using Nuke.Common.Tools.DotNet;

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

    Target Pack => target => target
        .Description("Packs all NuGet packages in the Solution")
        .DependsOn<IHaveCleanTarget>()
        .DependsOn<IHaveDotNetPackTarget>()
        .DependsOn<IHaveDotNetValidateTarget>();

    Target Publish => target => target
        .Description("Publishes all NuGet packages in the Solution")
        .DependsOn<IHaveDotNetPushTarget>();

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
