// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.MultiTenancy.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
///     Provides functionality to configure tenant management services.
/// </summary>
public interface ITenantManagementBuilder
{
    /// <summary>
    ///     Gets the <see cref="IServiceCollection"/> where tenant management services are configured.
    /// </summary>
    IServiceCollection Services { get; }
}
