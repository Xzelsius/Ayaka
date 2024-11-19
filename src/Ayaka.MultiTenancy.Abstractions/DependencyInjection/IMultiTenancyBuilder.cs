// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.MultiTenancy.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides functionality to configure multi-tenancy services.
/// </summary>
public interface IMultiTenancyBuilder
{
    /// <summary>
    /// Gets the <see cref="IServiceCollection"/> where multi-tenancy services are configured.
    /// </summary>
    IServiceCollection Services { get; }
}
