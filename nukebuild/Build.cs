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
        IHaveConfiguration,
        ICanClean,
        ICanRestoreDotNet,
        ICanCompileDotNet,
        ICanTestDotNet,
        ICanPackDotNet
{
    public static int Main() => Execute<Build>(x => ((ICanPackDotNet)x).Pack);
}
