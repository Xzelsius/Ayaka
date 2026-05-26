// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.Cake.Tests.PublicApi;

using Ayaka.Cake.PublicApi;

public sealed class IgnoreCaseWhenPossibleComparerTest
{
    [Fact]
    public void Does_sort_lines()
    {
        List<string> lines =
        [
            "#nullable enable",
            "Ayaka.Cake.DotNet.IHaveDotNetPackTarget",
            "Ayaka.Cake.DotNet.ICanDotNetBuild.DotNetBuildSettingsBase.get -> Cake.Common.Tools.DotNet.Build.DotNetBuildSettings!",
            "Ayaka.Cake.DotNet.ICanDotNetBuild",
            "Ayaka.Cake.Extensions",
            "Ayaka.Cake.DotNet.ICanDotNetBuild.DotNetBuildSettings.get -> Cake.Common.Tools.DotNet.Build.DotNetBuildSettings!"
        ];

        lines.Sort(IgnoreCaseWhenPossibleComparer.Instance);

        lines
            .Should()
            .BeEquivalentTo(
                [
                    "#nullable enable",
                    "Ayaka.Cake.DotNet.ICanDotNetBuild",
                    "Ayaka.Cake.DotNet.ICanDotNetBuild.DotNetBuildSettings.get -> Cake.Common.Tools.DotNet.Build.DotNetBuildSettings!",
                    "Ayaka.Cake.DotNet.ICanDotNetBuild.DotNetBuildSettingsBase.get -> Cake.Common.Tools.DotNet.Build.DotNetBuildSettings!",
                    "Ayaka.Cake.DotNet.IHaveDotNetPackTarget",
                    "Ayaka.Cake.Extensions"
                ],
                o => o.WithStrictOrdering());
    }

    [Fact]
    public void Does_fallback_to_case_sensitive()
    {
        List<string> lines =
        [
            "ayaka.cake.extensions",
            "Ayaka.Cake.DotNet.ICanDotNetBuild",
            "Ayaka.Cake.Extensions"
        ];

        lines.Sort(IgnoreCaseWhenPossibleComparer.Instance);

        lines
            .Should()
            .BeEquivalentTo(
                [
                    "Ayaka.Cake.DotNet.ICanDotNetBuild",
                    "Ayaka.Cake.Extensions",
                    "ayaka.cake.extensions",
                ],
                o => o.WithStrictOrdering());
    }
}
