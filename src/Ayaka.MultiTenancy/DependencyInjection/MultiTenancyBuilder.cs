// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.MultiTenancy.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Allows configuration of multi-tenancy services.
/// </summary>
internal sealed class MultiTenancyBuilder : IMultiTenancyBuilder
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MultiTenancyBuilder"/> class.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    public MultiTenancyBuilder(IServiceCollection services)
    {
        Services = services;
    }

    /// <inheritdoc />
    public IServiceCollection Services { get; }
}
