// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.MultiTenancy.DependencyInjection;

using Ayaka.MultiTenancy.Management;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

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
        services.TryAddSingleton<ITenantManager, DefaultTenantManager>();
    }
}
