// Copyright (c) Raphael Strotz. All rights reserved.

using Ayaka.Nuke;
using Ayaka.Nuke.DotNet;
using Ayaka.Nuke.DotNetValidate;
using Ayaka.Nuke.GitHub;
using Ayaka.Nuke.NuGet;
using Ayaka.Nuke.PublicApi;
using Nuke.Common;
using Nuke.Common.Tools.DotNet;

[DotNetVerbosityMapping]
class Build
    : NukeBuild,
        IHaveGitRepository,
        IHaveGitVersion,
        IHaveSolution,
        IHaveSources,
        IHaveTests,
        IHaveArtifacts,
        IHaveTestArtifacts,
        IHaveCodeCoverage,
        IHavePackageArtifacts,
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

    Target CreateRelease => target => target
        .Description("Creates a new GitHub Release based on the current commit")
        .DependsOn<IHaveGitHubReleaseTarget>();

    Target AfterRelease => target => target
        .Description("Ships all Public APIs")
        .DependsOn<IHaveShipPublicApisTarget>();
}
