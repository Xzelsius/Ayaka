// Copyright (c) Raphael Strotz and other contributors under the MIT license. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace Ayaka.Infrastructure
{
    /// <summary>
    ///     Extension methods for setting up an application using <see cref="IApplicationBuilder" />.
    /// </summary>
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        ///     Scans for <see cref="IApplicationConfigurer" /> in current <see cref="AppDomain" /> and applies configuration using
        ///     the found configurers.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder" />.</param>
        /// <returns>A reference to the app after the operation has completed.</returns>
        public static IApplicationBuilder Scan(this IApplicationBuilder app)
            => app.Scan(AppDomain.CurrentDomain.GetAssemblies());

        /// <summary>
        ///     Searches for <see cref="IApplicationConfigurer" /> in the specified assemblies and applies configuration using the
        ///     found configurers.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder" />.</param>
        /// <param name="assemblies">The assemblies to search for <see cref="IApplicationConfigurer" /> classes.</param>
        /// <returns>A reference to the app after the operation has completed.</returns>
        public static IApplicationBuilder Scan(this IApplicationBuilder app, params Assembly[] assemblies)
            => app.ScanForConfigurers(null, assemblies);

        /// <summary>
        ///     Searches for <see cref="IApplicationConfigurer" /> in the specified assemblies and applies configuration using the
        ///     found configurers.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder" />.</param>
        /// <param name="assemblies">The assemblies to search for <see cref="IApplicationConfigurer" /> classes.</param>
        /// <returns>A reference to the app after the operation has completed.</returns>
        public static IApplicationBuilder Scan(this IApplicationBuilder app, IEnumerable<Assembly> assemblies)
            => app.ScanForConfigurers(null, assemblies);

        /// <summary>
        ///     Scans for <see cref="IApplicationConfigurer" /> in current <see cref="AppDomain" /> and applies configuration using
        ///     the found configurers.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder" />.</param>
        /// <param name="env">An instance of <see cref="IHostingEnvironment" />.</param>
        /// <returns>A reference to the app after the operation has completed.</returns>
        public static IApplicationBuilder Scan(this IApplicationBuilder app, IHostingEnvironment env)
            => app.Scan(env, AppDomain.CurrentDomain.GetAssemblies());

        /// <summary>
        ///     Searches for <see cref="IApplicationConfigurer" /> in the specified assemblies and applies configuration using the
        ///     found configurers.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder" />.</param>
        /// <param name="env">An instance of <see cref="IHostingEnvironment" />.</param>
        /// <param name="assemblies">The assemblies to search for <see cref="IApplicationConfigurer" /> classes.</param>
        /// <returns>A reference to the app after the operation has completed.</returns>
        public static IApplicationBuilder Scan(this IApplicationBuilder app, IHostingEnvironment env, params Assembly[] assemblies)
            => app.ScanForConfigurers(env, assemblies);

        /// <summary>
        ///     Searches for <see cref="IApplicationConfigurer" /> in the specified assemblies and applies configuration using the
        ///     found configurers.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder" />.</param>
        /// <param name="env">An instance of <see cref="IHostingEnvironment" />.</param>
        /// <param name="assemblies">The assemblies to search for <see cref="IApplicationConfigurer" /> classes.</param>
        /// <returns>A reference to the app after the operation has completed.</returns>
        public static IApplicationBuilder Scan(this IApplicationBuilder app, IHostingEnvironment env, IEnumerable<Assembly> assemblies)
            => app.ScanForConfigurers(env, assemblies);

        private static IApplicationBuilder ScanForConfigurers(this IApplicationBuilder app, IHostingEnvironment env, IEnumerable<Assembly> assemblies)
        {
            assemblies = assemblies as Assembly[] ?? assemblies.ToArray();

            var allTypes = assemblies
                .SelectMany(a => a.DefinedTypes)
                .ToArray();

            var registrarTypes = allTypes
                .Where(t => !t.IsAbstract)
                .Where(t => typeof(IApplicationConfigurer).GetTypeInfo().IsAssignableFrom(t))
                .ToArray();

            var configurerInstances = registrarTypes
                .Select(Activator.CreateInstance)
                .Cast<IApplicationConfigurer>()
                .OrderBy(ac => ac.Order);

            foreach (var configurer in configurerInstances)
            {
                configurer.Configure(app, env);
            }

            return app;
        }
    }
}
