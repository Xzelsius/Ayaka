// Copyright (c) Raphael Strotz. All rights reserved.

using Cake.DotNetLocalTools.Module;
using Cake.Frosting;

namespace Ayaka.Cake;

/// <summary>
///     Extension methods on <see cref="CakeHost" /> for hosting Ayaka.Cake builds.
/// </summary>
public static class CakeHostExtensions
{
    /// <summary>
    ///     Registers Ayaka.Cake against the supplied <typeparamref name="TContext" />:
    ///     binds the context, scans the Ayaka.Cake assembly for tasks, wires the
    ///     local-tools Cake module, and installs the tools listed in
    ///     <c>.config/dotnet-tools.json</c>.
    /// </summary>
    /// <typeparam name="TContext">The consumer's <see cref="IAyakaContext" /> implementation.</typeparam>
    /// <param name="host">The Cake host to configure.</param>
    /// <returns>The same <paramref name="host" /> instance, for chaining.</returns>
    [ExcludeFromCodeCoverage]
    public static CakeHost UseAyaka<TContext>(this CakeHost host)
        where TContext : class, IAyakaContext
    {
        return host
            .UseContext<TContext>()
            .UseModule<LocalToolsModule>()
            .InstallToolsFromManifest(".config/dotnet-tools.json")
            .AddAssembly(typeof(IAyakaContext).Assembly);
    }
}
