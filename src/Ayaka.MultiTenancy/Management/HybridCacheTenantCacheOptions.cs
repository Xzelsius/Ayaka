// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.MultiTenancy.Management;

using System.ComponentModel.DataAnnotations;

/// <summary>
///     Provides configuration options for caching tenants in a <see cref="Microsoft.Extensions.Caching.Hybrid.HybridCache"/>.
/// </summary>
public sealed class HybridCacheTenantCacheOptions
{
    /// <summary>
    ///     Gets or sets how long a tenant remains cached.
    /// </summary>
    /// <remarks>
    ///     Must be between 1 minute and 1 hour. Defaults to 5 minutes.
    /// </remarks>
    [Range(typeof(TimeSpan), "00:01:00", "01:00:00")]
    public TimeSpan Expiration { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    ///     Gets or sets a prefix that scopes the cached tenants to this application.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Applications that share a distributed cache share their cached tenants and evictions unless each
    ///         sets its own prefix. The prefix is added to Ayaka's own cache keys and tag; it doesn't replace them.
    ///     </para>
    ///     <para>
    ///         May contain only ASCII letters, digits, <c>-</c>, <c>_</c>, and <c>.</c>, and must be at most 64
    ///         characters long. Defaults to <see langword="null"/>; an empty string is treated the same and means no
    ///         prefix.
    ///     </para>
    /// </remarks>
    [MaxLength(64)]
    [RegularExpression("^[A-Za-z0-9._-]+$")]
    public string? KeyPrefix { get; set; }
}
