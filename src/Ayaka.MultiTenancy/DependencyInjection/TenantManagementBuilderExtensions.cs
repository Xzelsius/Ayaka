// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.MultiTenancy.DependencyInjection;

using Ayaka.MultiTenancy.Management;
using Microsoft.Extensions.DependencyInjection.Extensions;

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
        builder.Services.TryAddSingleton<ITenantStore, InMemoryTenantStore>();

        return builder;
    }
}
