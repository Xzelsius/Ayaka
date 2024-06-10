// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.Nuke;

using global::Nuke.Common;
using global::Nuke.Common.Tools.GitVersion;

/// <summary>
///     Provides versioning information using <c>GitVersion</c> based on the current
///     git repository in a <see cref="INukeBuild" />.
/// </summary>
/// <remarks>
///     Please make sure to have <c>GitVersion.Tool</c> available to your build environment,
///     either by installing it globally or by adding it as a local tool to your project.
///     <para>
///         Check out the following links
///         <list type="bullet">
///             <item>
///                 <description>
///                     <see href="https://nuke.build/docs/common/versioning/" /> for more information regarding
///                     versioning in a <see cref="INukeBuild" />.
///                 </description>
///             </item>
///             <item>
///                 <description>
///                     <see href="https://gitversion.net" /> for more information regarding <c>GitVersion</c>
///                 </description>
///             </item>
///         </list>
///     </para>
/// </remarks>
public interface IHaveGitVersion : IHave
{
    /// <summary>
    ///     Gets the versioning information from <c>GitVersion</c>.
    /// </summary>
    [GitVersion(NoFetch = true)]
    [Required]
    [ExcludeFromCodeCoverage]
    GitVersion Versioning => TryGetValue(() => Versioning)!;
}
