// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.MultiTenancy.DependencyInjection;

/// <summary>
///     Provides functionality to configure tenant management services.
/// </summary>
public interface ITenantManagementBuilder
{
    /// <summary>
    ///     Gets the <see cref="IMultiTenancyBuilder"/> where tenant management services are configured.
    /// </summary>
    IMultiTenancyBuilder MultiTenancy { get; }
}
