// Copyright (c) Raphael Strotz and other contributors under the MIT license. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Ayaka.Infrastructure
{
    /// <summary>
    ///     Extension methods for setting up services in an <see cref="IServiceCollection" />.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        ///     Scans for <see cref="IServiceRegistrar" /> in current <see cref="AppDomain" /> and adds services using the found
        ///     registrars.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
        /// <returns>The same service collection so that multiple calls can be chained.</returns>
        public static IServiceCollection Scan(this IServiceCollection services)
            => services.Scan(AppDomain.CurrentDomain.GetAssemblies());

        /// <summary>
        ///     Searches for <see cref="IServiceRegistrar" /> in the specified assemblies and adds services using the found
        ///     registrars.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
        /// <param name="assemblies">The assemblies to search for <see cref="IServiceRegistrar" /> classes.</param>
        /// <returns>The same service collection so that multiple calls can be chained.</returns>
        public static IServiceCollection Scan(this IServiceCollection services, params Assembly[] assemblies)
            => services.ScanForRegistrars(assemblies);

        /// <summary>
        ///     Searches for <see cref="IServiceRegistrar" /> in the specified assemblies and adds services using the found
        ///     registrars.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
        /// <param name="assemblies">The assemblies to search for <see cref="IServiceRegistrar" /> classes.</param>
        /// <returns>The same service collection so that multiple calls can be chained.</returns>
        public static IServiceCollection Scan(this IServiceCollection services, IEnumerable<Assembly> assemblies)
            => services.ScanForRegistrars(assemblies);

        private static IServiceCollection ScanForRegistrars(this IServiceCollection services, IEnumerable<Assembly> assemblies)
        {
            assemblies = assemblies as Assembly[] ?? assemblies.ToArray();

            var allTypes = assemblies
                .SelectMany(a => a.DefinedTypes)
                .ToArray();

            var registrarTypes = allTypes
                .Where(t => !t.IsAbstract)
                .Where(t => typeof(IServiceRegistrar).GetTypeInfo().IsAssignableFrom(t))
                .ToArray();

            var registrarInstances = registrarTypes
                .Select(Activator.CreateInstance)
                .Cast<IServiceRegistrar>()
                .OrderBy(sr => sr.Order);

            foreach (var registrar in registrarInstances)
            {
                registrar.ConfigureServices(services);
            }

            return services;
        }
    }
}
