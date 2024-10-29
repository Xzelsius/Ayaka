// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.MultiTenancy;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

/// <summary>
///     Extension methods for <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
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
}
