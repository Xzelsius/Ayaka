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
    /// <remarks>
    ///     Must be between 1 minute and 1 hour. Defaults to 5 minutes.
    /// </remarks>
    public TimeSpan Expiration { get; set; } = TimeSpan.FromMinutes(5);
}
