// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.MultiTenancy;

using Ayaka.MultiTenancy.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

/// <summary>
///     Extension methods for <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Adds multi-tenancy services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <remarks>
    ///     In order to fine-tune the multi-tenancy configuration, use the <see cref="IMultiTenancyBuilder"/> returned by
    ///     this method.
    /// </remarks>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the service to.</param>
    /// <returns>A <see cref="IMultiTenancyBuilder"/> that can be used to further configure multi-tenancy.</returns>
    public static IMultiTenancyBuilder AddMultiTenancy(this IServiceCollection services)
    {
        ConfigureDefaultServices(services);

        return new MultiTenancyBuilder(services);
    }

    /// <summary>
    ///     Adds the default implementation for the <see cref="ITenantContextAccessor"/> service.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the service to.</param>
    /// <returns>The same <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddTenantContextAccessor(this IServiceCollection services)
    {
        services.TryAddSingleton<ITenantContextAccessor, AsyncLocalTenantContextAccessor>();
        return services;
    }

    [SuppressMessage("Style", "IDE0058:Expression value is never used")]
    private static void ConfigureDefaultServices(this IServiceCollection services)
    {
        // The heart of multi-tenancy
        services.AddTenantContextAccessor();
    }
}
