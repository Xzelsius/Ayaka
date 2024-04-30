// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.Nuke;

using global::Nuke.Common;

/// <summary>
///     Marker to indicate that a <see cref="INukeBuild" /> is capable to perform something.
///     <para>
///         Mostly used to provide additional <see cref="ITargetDefinition" />s to a build.
///     </para>
/// </summary>
public interface ICan : INukeBuild
{
}
