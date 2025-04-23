// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.MultiTenancy.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
///     Allows configuration of tenant management services.
/// </summary>
internal sealed class TenantManagementBuilder : ITenantManagementBuilder
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="TenantManagementBuilder"/> class.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    public TenantManagementBuilder(IServiceCollection services)
    {
        Services = services;
    }

    /// <inheritdoc />
    public IServiceCollection Services { get; }
}
