// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.Build;

public sealed class BuildRunner
{
    internal BuildRunner()
    {
    }

    public static BuildRunner Create<TBuildDefinition>()
        where TBuildDefinition : BuildDefinition, new()
        => Create(new TBuildDefinition());

    public static BuildRunner Create(BuildDefinition definition)
    {
        return new BuildRunner();
    }

    public async Task<int> RunAsync(string[] args)
    {
        try
        {
            return 0;
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e);
            return 1;
        }
    }
}
