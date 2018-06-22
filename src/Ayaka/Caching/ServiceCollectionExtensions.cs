// Copyright (c) Raphael Strotz and other contributors under the MIT license. All rights reserved.

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Ayaka.Caching
{
    /// <summary>
    ///     Extension methods for setting up services in an <see cref="IServiceCollection" />.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        ///     Adds the specified cache to the service collection.
        /// </summary>
        /// <typeparam name="TCache">The type of the cache.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
        /// <returns>The same service collection so that multiple calls can be chained.</returns>
        public static IServiceCollection AddCache<TCache>(this IServiceCollection services) 
            where TCache : class, ICache
        {
            services.AddOptions();
            services.TryAddSingleton<ICache, TCache>();

            return services;
        }

        /// <summary>
        ///     Adds the specified cache and options to the service collection.
        /// </summary>
        /// <typeparam name="TCache">The type of the cache.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
        /// <param name="optionsAction">An optional action to configure the cache.</param>
        /// <returns>The same service collection so that multiple calls can be chained.</returns>
        public static IServiceCollection AddCache<TCache>(this IServiceCollection services, Action<CacheOptions> optionsAction) 
            where TCache : class, ICache
        {
            services.AddCache<TCache>();
            services.Configure(optionsAction);

            return services;
        }
    }
}
