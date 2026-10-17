// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.MultiTenancy.DependencyInjection;

using Ayaka.MultiTenancy.Management;
using Light.GuardClauses;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

/// <summary>
///     Extension methods for <see cref="ITenantManagementBuilder"/>.
/// </summary>
public static class TenantManagementBuilderExtensions
{
    /// <summary>
    ///     Configures the tenant management to use an in-memory store
    ///     for managing available tenants.
    /// </summary>
    /// <param name="builder">The <see cref="ITenantManagementBuilder"/> to configure.</param>
    /// <returns>The same <see cref="ITenantManagementBuilder"/>.</returns>
    public static ITenantManagementBuilder UseInMemoryStore(this ITenantManagementBuilder builder)
    {
        builder.MustNotBeNull();

        builder.MultiTenancy.Services.TryAddSingleton<ITenantStore, InMemoryTenantStore>();

        return builder;
    }

    /// <summary>
    ///     Configures the tenant management to cache tenants in a <see cref="HybridCache"/>.
    /// </summary>
    /// <remarks>
    ///     Tenants are cached in memory, and additionally in the registered <see cref="IDistributedCache"/>, if any.
    /// </remarks>
    /// <param name="builder">The <see cref="ITenantManagementBuilder"/> to configure.</param>
    /// <param name="configure">An optional action to configure the <see cref="TenantCacheOptions"/>.</param>
    /// <returns>The same <see cref="ITenantManagementBuilder"/>.</returns>
    public static ITenantManagementBuilder UseHybridCache(
        this ITenantManagementBuilder builder,
        Action<TenantCacheOptions>? configure = null)
    {
        builder.MustNotBeNull();

        builder.MultiTenancy.Services.AddHybridCache();

        return builder.UseTenantCache(ServiceDescriptor.Singleton<ITenantCache, HybridCacheTenantCache>(), configure);
    }

    /// <summary>
    ///     Configures the tenant management to cache tenants in the keyed <see cref="HybridCache"/>
    ///     registered under <paramref name="serviceKey"/>.
    /// </summary>
    /// <remarks>
    ///     Use this to keep tenants in a different cache than the rest of the application. The keyed
    ///     <see cref="HybridCache"/> must be registered by the caller, e.g. with
    ///     <c>AddKeyedHybridCache(serviceKey, options => options.DistributedCacheServiceKey = ...)</c>
    ///     to back it by a keyed <see cref="IDistributedCache"/>.
    /// </remarks>
    /// <param name="builder">The <see cref="ITenantManagementBuilder"/> to configure.</param>
    /// <param name="serviceKey">The service key of the <see cref="HybridCache"/> to use.</param>
    /// <param name="configure">An optional action to configure the <see cref="TenantCacheOptions"/>.</param>
    /// <returns>The same <see cref="ITenantManagementBuilder"/>.</returns>
    public static ITenantManagementBuilder UseKeyedHybridCache(
        this ITenantManagementBuilder builder,
        object serviceKey,
        Action<TenantCacheOptions>? configure = null)
    {
        builder.MustNotBeNull();
        serviceKey.MustNotBeNull();

        return builder.UseTenantCache(
            ServiceDescriptor.Singleton<ITenantCache>(services => new HybridCacheTenantCache(
                services.GetRequiredKeyedService<HybridCache>(serviceKey),
                services.GetRequiredService<IOptions<TenantCacheOptions>>(),
                services.GetRequiredService<TimeProvider>())),
            configure);
    }

    private static ITenantManagementBuilder UseTenantCache(
        this ITenantManagementBuilder builder,
        ServiceDescriptor cache,
        Action<TenantCacheOptions>? configure)
    {
        var services = builder.MultiTenancy.Services;

        services.AddOptionsWithValidateOnStart<TenantCacheOptions, TenantCacheOptionsAutoValidator>();
        if (configure is not null)
        {
            services.Configure(configure);
        }

        // Replace the default cache, but keep a cache registered by the user
        var defaultCache = services.FirstOrDefault(x =>
            x.ServiceType == typeof(ITenantCache) && x.ImplementationType == typeof(NoopTenantCache));
        if (defaultCache is not null)
        {
            services.Remove(defaultCache);
        }

        services.TryAdd(cache);

        return builder;
    }
}
