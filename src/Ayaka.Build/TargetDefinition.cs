// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.Build;

using System.Diagnostics;
using System.Reflection;

/// <summary>
///     Captures the configuration of a single build target while a <see cref="Target"/>
///     delegate is invoked.
/// </summary>
[DebuggerDisplay("{Name}")]
internal sealed class TargetDefinition : ITargetDefinition
{
    private readonly PropertyInfo _target;
    private readonly List<Func<Task>> _actions = [];

    /// <summary>
    ///     Initializes a new instance of the <see cref="TargetDefinition"/> class.
    /// </summary>
    /// <param name="target">The property declaring the target this definition configures.</param>
    public TargetDefinition(PropertyInfo target)
    {
        _target = target;
    }

    /// <summary>
    ///     Gets the name of the target.
    /// </summary>
    /// <value>A string containing the target name.</value>
    public string Name => _target.Name;

    /// <inheritdoc />
    public ITargetDefinition Executes(Action action)
        => Executes(() =>
        {
            action();
            return Task.CompletedTask;
        });

    /// <inheritdoc />
    public ITargetDefinition Executes(Func<Task> action)
    {
        _actions.Add(action);
        return this;
    }
}
