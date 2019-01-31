// Copyright (c) Raphael Strotz and other contributors under the MIT license. All rights reserved.

using System;
using Ayaka.Caching.Memory;
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
        ///     Adds the specified <typeparamref name="TCacheManager">cache manager</typeparamref> to the service collection.
        /// </summary>
        /// <typeparam name="TCacheManager">The type of the cache manager.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
        /// <returns>The same service collection so that multiple calls can be chained.</returns>
        public static IServiceCollection AddCacheManager<TCacheManager>(this IServiceCollection services)
            where TCacheManager : class, ICacheManager
        {
            services.AddOptions();
            services.TryAddSingleton<ICacheManager, TCacheManager>();

            return services;
        }

        /// <summary>
        ///     Adds the specified <typeparamref name="TCacheManager">cache manager</typeparamref> and options to the service
        ///     collection.
        /// </summary>
        /// <typeparam name="TCacheManager">The type of the cache manager.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
        /// <param name="setupAction">The action to configure the cache manager.</param>
        /// <returns>The same service collection so that multiple calls can be chained.</returns>
        public static IServiceCollection AddCacheManager<TCacheManager>(this IServiceCollection services, Action<CacheOptions> setupAction)
            where TCacheManager : class, ICacheManager
        {
            services.AddCacheManager<TCacheManager>();
            services.Configure(setupAction);

            return services;
        }

        /// <summary>
        ///     Adds a <see cref="MemoryCacheManager" /> to the service collection.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
        /// <returns>The same service collection so that multiple calls can be chained.</returns>
        public static IServiceCollection AddMemoryCacheManager(this IServiceCollection services)
        {
            services.AddCacheManager<MemoryCacheManager>();
            return services;
        }

        /// <summary>
        ///     Adds a <see cref="MemoryCacheManager" /> and specified options to the service collection.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
        /// <param name="setupAction">The action to configure the cache manager.</param>
        /// <returns>The same service collection so that multiple calls can be chained.</returns>
        public static IServiceCollection AddMemoryCacheManager(this IServiceCollection services, Action<CacheOptions> setupAction)
        {
            services.AddCacheManager<MemoryCacheManager>(setupAction);
            return services;
        }
    }
}