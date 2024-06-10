// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.Nuke.Tests.PublicApi;

using Ayaka.Nuke.PublicApi;

public sealed class IgnoreCaseWhenPossibleComparerTest
{
    [Fact]
    public void Does_sort_lines()
    {
        List<string> lines =
        [
            "#nullable enable",
            "Ayaka.Nuke.DotNet.IHaveDotNetPackTarget",
            "Ayaka.Nuke.DotNet.ICanDotNetBuild.DotNetBuildSettingsBase.get -> Nuke.Common.Tooling.Configure<Nuke.Common.Tools.DotNet.DotNetBuildSettings!>!",
            "Ayaka.Nuke.DotNet.ICanDotNetBuild",
            "ayaka.nuke.extensions",
            "Ayaka.Nuke.DotNet.ICanDotNetBuild.DotNetBuildSettings.get -> Nuke.Common.Tooling.Configure<Nuke.Common.Tools.DotNet.DotNetBuildSettings!>!"
        ];

        lines.Sort(IgnoreCaseWhenPossibleComparer.Instance);

        lines
            .Should()
            .BeEquivalentTo(
                [
                    "#nullable enable",
                    "Ayaka.Nuke.DotNet.ICanDotNetBuild",
                    "Ayaka.Nuke.DotNet.ICanDotNetBuild.DotNetBuildSettings.get -> Nuke.Common.Tooling.Configure<Nuke.Common.Tools.DotNet.DotNetBuildSettings!>!",
                    "Ayaka.Nuke.DotNet.ICanDotNetBuild.DotNetBuildSettingsBase.get -> Nuke.Common.Tooling.Configure<Nuke.Common.Tools.DotNet.DotNetBuildSettings!>!",
                    "Ayaka.Nuke.DotNet.IHaveDotNetPackTarget",
                    "ayaka.nuke.extensions"
                ],
                o => o.WithStrictOrdering());
    }
}
