// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.Build.Execution;

using System.Diagnostics;

/// <summary>
///     Represents an executable build target.
/// </summary>
[DebuggerDisplay("{Name}")]
internal sealed class BuildTarget
{
    private readonly TargetDefinition _definition;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BuildTarget"/> class.
    /// </summary>
    /// <param name="definition">The underlying target definition.</param>
    public BuildTarget(TargetDefinition definition)
    {
        _definition = definition;
    }

    /// <summary>
    ///     Gets the name of the build target.
    /// </summary>
    /// <value>A string containing the build target name.</value>
    public string Name => _definition.Name;
}
