// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.MultiTenancy.DependencyInjection;

/// <summary>
///     Allows configuration of tenant management services.
/// </summary>
internal sealed class TenantManagementBuilder : ITenantManagementBuilder
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="TenantManagementBuilder"/> class.
    /// </summary>
    /// <param name="multiTenancy">The <see cref="IMultiTenancyBuilder" /> to configure tenant management on.</param>
    public TenantManagementBuilder(IMultiTenancyBuilder multiTenancy)
    {
        MultiTenancy = multiTenancy;
    }

    /// <inheritdoc />
    public IMultiTenancyBuilder MultiTenancy { get; }
}
