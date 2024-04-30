// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.Nuke;

using global::Nuke.Common;

/// <summary>
///     Marker to indicate that a <see cref="INukeBuild" /> does provide something.
///     <para>
///         Mostly used to provide additional parameters, directories or attributes for a build.
///     </para>
/// </summary>
public interface IHave : INukeBuild
{
}
