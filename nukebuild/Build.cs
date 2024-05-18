// Copyright (c) Raphael Strotz. All rights reserved.

using Ayaka.Nuke;
using Ayaka.Nuke.DotNet;
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
        IHavePackageArtifacts,
        IHaveDotNetConfiguration,
        ICanClean,
        ICanDotNetRestore,
        ICanDotNetBuild,
        ICanDotNetTest,
        ICanDotNetPack
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
        .DependsOn<IHaveDotNetPackTarget>();
}
