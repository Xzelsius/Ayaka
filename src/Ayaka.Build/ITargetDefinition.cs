// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.Build;

/// <summary>
///     A build target definition.
/// </summary>
public interface ITargetDefinition
{
    /// <summary>
    ///     Executes the <paramref name="action"/> as part of this target.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <returns>The same <see cref="ITargetDefinition"/> for chaining.</returns>
    ITargetDefinition Executes(Action action);

    /// <summary>
    ///     Executes the <paramref name="action"/> as part of this target.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <returns>The same <see cref="ITargetDefinition"/> for chaining.</returns>
    ITargetDefinition Executes(Func<Task> action);
}
