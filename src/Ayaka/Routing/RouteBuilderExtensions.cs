// Copyright (c) Raphael Strotz and other contributors under the MIT license. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Routing;

namespace Ayaka.Routing
{
    /// <summary>
    ///     Extension methods for setting up routes in an <see cref="IRouteBuilder" />.
    /// </summary>
    public static class RouteBuilderExtensions
    {
        /// <summary>
        ///     Scans for <see cref="IRouteRegistrar" /> in current <see cref="AppDomain" /> and configures routes using the found
        ///     registrars.
        /// </summary>
        /// <param name="builder">The <see cref="IRouteBuilder" /> to configure.</param>
        /// <returns>The same route builder so that multiple calls can be chained.</returns>
        public static IRouteBuilder Scan(this IRouteBuilder builder)
            => builder.Scan(AppDomain.CurrentDomain.GetAssemblies());

        /// <summary>
        ///     Scans for <see cref="IRouteRegistrar" /> in the specified assemblies and configures routes using the found
        ///     registrars.
        /// </summary>
        /// <param name="builder">The <see cref="IRouteBuilder" /> to configure.</param>
        /// <param name="assemblies">The assemblies to search for <see cref="IRouteRegistrar" /> classes.</param>
        /// <returns>The same route builder so that multiple calls can be chained.</returns>
        public static IRouteBuilder Scan(this IRouteBuilder builder, params Assembly[] assemblies)
            => builder.ScanForRegistrars(assemblies);

        /// <summary>
        ///     Scans for <see cref="IRouteRegistrar" /> in the specified assemblies and configures routes using the found
        ///     registrars.
        /// </summary>
        /// <param name="builder">The <see cref="IRouteBuilder" /> to configure.</param>
        /// <param name="assemblies">The assemblies to search for <see cref="IRouteRegistrar" /> classes.</param>
        /// <returns>The same route builder so that multiple calls can be chained.</returns>
        public static IRouteBuilder Scan(this IRouteBuilder builder, IEnumerable<Assembly> assemblies)
            => builder.ScanForRegistrars(assemblies);

        private static IRouteBuilder ScanForRegistrars(this IRouteBuilder builder, IEnumerable<Assembly> assemblies)
        {
            assemblies = assemblies as Assembly[] ?? assemblies.ToArray();

            var allTypes = assemblies
                .SelectMany(a => a.DefinedTypes)
                .ToArray();

            var registrarTypes = allTypes
                .Where(t => !t.IsAbstract)
                .Where(t => typeof(IRouteRegistrar).GetTypeInfo().IsAssignableFrom(t))
                .ToArray();

            var registrarInstances = registrarTypes
                .Select(Activator.CreateInstance)
                .Cast<IRouteRegistrar>()
                .OrderBy(rr => rr.Order);

            foreach (var registrar in registrarInstances)
            {
                registrar.ConfigureRoutes(builder);
            }

            return builder;
        }
    }
}
