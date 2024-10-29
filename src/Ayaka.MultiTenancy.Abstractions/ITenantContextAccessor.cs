// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.MultiTenancy;

/// <summary>
///     Provides access to the current <see cref="TenantContext"/>, if one is available.
/// </summary>
public interface ITenantContextAccessor
{
    /// <summary>
    ///     Gets or sets the information about the current tenant.
    /// </summary>
    TenantContext? TenantContext { get; set; }
}
