// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.Build;

using Ayaka.Build.Execution;

/// <summary>
///     Runs the targets declared by a <see cref="BuildDefinition"/>.
/// </summary>
public sealed class BuildRunner
{
    private readonly BuildModel _model;

    internal BuildRunner(BuildModel model)
    {
        _model = model;
    }

    /// <summary>
    ///     Creates a <see cref="BuildRunner"/> for a new instance of the
    ///     <typeparamref name="TBuildDefinition"/> build definition.
    /// </summary>
    /// <typeparam name="TBuildDefinition">The type of the build definition to run.</typeparam>
    /// <returns>A new <see cref="BuildRunner"/> instance.</returns>
    public static BuildRunner Create<TBuildDefinition>()
        where TBuildDefinition : BuildDefinition, new()
        => Create(new TBuildDefinition());

    /// <summary>
    ///     Creates a <see cref="BuildRunner"/> for the specified <paramref name="definition"/>.
    /// </summary>
    /// <param name="definition">The build definition to run.</param>
    /// <returns>A new <see cref="BuildRunner"/> instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="definition"/> is <see langword="null"/>.</exception>
    public static BuildRunner Create(BuildDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        var model = BuildModel.FromDefinition(definition);
        return new BuildRunner(model);
    }

    /// <summary>
    ///     Runs the build with the given <paramref name="args"/>.
    /// </summary>
    /// <param name="args">The command-line arguments for the run.</param>
    /// <returns>
    ///     A task that resolves to the process exit code:
    ///     <c>0</c> on success, <c>1</c> on failure.
    /// </returns>
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
