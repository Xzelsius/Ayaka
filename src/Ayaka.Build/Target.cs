// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.Build;

/// <summary>
///     Defines a build target by configuring the supplied <see cref="ITargetDefinition"/>.
/// </summary>
/// <param name="definition">The target definition to configure.</param>
/// <returns>The configured <see cref="ITargetDefinition"/>.</returns>
public delegate ITargetDefinition Target(ITargetDefinition definition);
