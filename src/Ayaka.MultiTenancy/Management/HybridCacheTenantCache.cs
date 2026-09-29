// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.MultiTenancy.Management;

using System.ComponentModel;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Options;

/// <summary>
///     Caches tenant instances in a <see cref="HybridCache"/> with per-tenant keys
///     and shared tenant tagging for invalidation.
/// </summary>
internal sealed class HybridCacheTenantCache : ITenantCache
{
    private const string KeyPrefix = "ayaka:tenant:";
    private const string TenantsTag = "ayaka:tenants";

    private static readonly string[] _tags = [TenantsTag];

    private readonly HybridCache _cache;
    private readonly HybridCacheEntryOptions _entryOptions;

    /// <summary>
    ///     Initializes a new instance of the <see cref="HybridCacheTenantCache"/> class
    /// </summary>
    /// <param name="cache">The <see cref="HybridCache"/> used to store and retrieve tenant data.</param>
    /// <param name="options">The <see cref="TenantCacheOptions"/> providing expiration settings for cache entries.</param>
    public HybridCacheTenantCache(
        HybridCache cache,
        IOptions<TenantCacheOptions> options)
    {
        _cache = cache;
        _entryOptions = new HybridCacheEntryOptions
        {
            Expiration = options.Value.Expiration,
            LocalCacheExpiration = options.Value.Expiration,
        };
    }

    /// <inheritdoc />
    public async ValueTask<Tenant?> GetOrCreateAsync(
        string id,
        Func<string, CancellationToken, ValueTask<Tenant?>> factory,
        CancellationToken cancellationToken = default)
    {
        var entry = await _cache.GetOrCreateAsync(
            KeyPrefix + id,
            state: (Id: id, Factory: factory),
            factory: static async (state, ct) => new TenantCacheEntry(await state.Factory(state.Id, ct)),
            _entryOptions,
            _tags,
            cancellationToken);

        return entry.Tenant;
    }
    
    /// <inheritdoc />
    public ValueTask EvictAsync(string id, CancellationToken cancellationToken = default)
        => _cache.RemoveAsync(KeyPrefix + id, cancellationToken);

    /// <inheritdoc />
    public ValueTask EvictAllAsync(CancellationToken cancellationToken = default)
        => _cache.RemoveByTagAsync(TenantsTag, cancellationToken);

    [ImmutableObject(true)]
    private sealed record TenantCacheEntry(Tenant? Tenant);
}
