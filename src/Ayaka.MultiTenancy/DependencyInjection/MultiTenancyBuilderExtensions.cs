// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.MultiTenancy.DependencyInjection;

using Ayaka.MultiTenancy.Management;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

/// <summary>
///     Extension methods for <see cref="IMultiTenancyBuilder"/>.
/// </summary>
public static class MultiTenancyBuilderExtensions
{
    /// <summary>
    ///     Adds tenant management services to the specified <see cref="IMultiTenancyBuilder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IMultiTenancyBuilder"/> to add the services to.</param>
    /// <returns>A <see cref="ITenantManagementBuilder"/> that can be used to further configure tenant management.</returns>
    public static ITenantManagementBuilder AddTenantManagement(this IMultiTenancyBuilder builder)
    {
        ConfigureDefaultServices(builder.Services);

        return new TenantManagementBuilder(builder);
    }

    private static void ConfigureDefaultServices(this IServiceCollection services)
    {
        _ = services.AddHybridCache();
        _ = services.AddOptions<TenantCacheOptions>().ValidateOnStart();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IValidateOptions<TenantCacheOptions>, TenantCacheOptionsValidator>());
        services.TryAddSingleton<ITenantCache, HybridCacheTenantCache>();
        services.TryAddSingleton<ITenantManager, DefaultTenantManager>();
    }
}
