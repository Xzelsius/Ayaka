// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.MultiTenancy.Management;

/// <summary>
///     Provides configuration options for tenant caching.
/// </summary>
public sealed class TenantCacheOptions
{
    /// <summary>
    ///     Gets or sets how long a tenant remains cached.
    /// </summary>
    public TimeSpan Expiration { get; set; } = TimeSpan.FromMinutes(5);
}
