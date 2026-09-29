// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.MultiTenancy.Management;

using System.Collections.Concurrent;
using System.ComponentModel;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Options;

/// <summary>
///     Caches tenant instances in a <see cref="HybridCache"/> with per-tenant keys
///     and shared tenant tagging for invalidation.
/// </summary>
/// <remarks>
///     <see cref="HybridCache"/> stores the result of a load even when the tenant was evicted while the load was
///     still running. Every entry therefore remembers when its load started, and an entry loaded before the latest
///     eviction of its tenant is discarded and loaded again. Evictions are only known to this instance; other nodes
///     sharing a distributed cache keep their local copy until it expires.
/// </remarks>
internal sealed class HybridCacheTenantCache : ITenantCache
{
    private const string KeyPrefix = "ayaka:tenant:";
    private const string TenantsTag = "ayaka:tenants";
    private const int MaxLoadAttempts = 3;

    private static readonly string[] _tags = [TenantsTag];

    private readonly HybridCache _cache;
    private readonly HybridCacheEntryOptions _entryOptions;
    private readonly long _evictionRetentionTicks;
    private readonly ConcurrentDictionary<string, long> _evictedAt = new(StringComparer.Ordinal);
    private long _allEvictedAt;

    /// <summary>
    ///     Initializes a new instance of the <see cref="HybridCacheTenantCache"/> class.
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

        // An eviction only matters while entries loaded before it can still be served
        _evictionRetentionTicks = (options.Value.Expiration * 2).Ticks;
    }

    /// <inheritdoc />
    public async ValueTask<Tenant?> GetOrCreateAsync(
        string id,
        Func<string, CancellationToken, ValueTask<Tenant?>> factory,
        CancellationToken cancellationToken = default)
    {
        var key = KeyPrefix + id;

        for (var attempt = 1; ; attempt++)
        {
            var entry = await _cache.GetOrCreateAsync(
                key,
                state: (Id: id, Factory: factory),
                factory: static async (state, ct) =>
                {
                    // Taken before the load, so an eviction during the load marks its result as stale
                    var loadedAt = UtcNowTicks();
                    return new TenantCacheEntry(await state.Factory(state.Id, ct), loadedAt);
                },
                _entryOptions,
                _tags,
                cancellationToken);

            if (!IsStale(id, entry) || attempt == MaxLoadAttempts)
            {
                return entry.Tenant;
            }

            await _cache.RemoveAsync(key, cancellationToken);
        }
    }

    /// <inheritdoc />
    public ValueTask EvictAsync(string id, CancellationToken cancellationToken = default)
    {
        var now = UtcNowTicks();
        _ = _evictedAt.AddOrUpdate(
            id,
            static (_, ticks) => ticks,
            static (_, previous, ticks) => Math.Max(previous, ticks),
            now);
        PruneEvictions(now);

        return _cache.RemoveAsync(KeyPrefix + id, cancellationToken);
    }

    /// <inheritdoc />
    public ValueTask EvictAllAsync(CancellationToken cancellationToken = default)
    {
        Volatile.Write(ref _allEvictedAt, UtcNowTicks());

        return _cache.RemoveByTagAsync(TenantsTag, cancellationToken);
    }

    // Wall-clock rather than a process-local counter, because entries read from a distributed cache were loaded elsewhere
    private static long UtcNowTicks()
        => DateTime.UtcNow.Ticks;

    private bool IsStale(string id, TenantCacheEntry entry)
        => entry.LoadedAt <= Volatile.Read(ref _allEvictedAt)
           || (_evictedAt.TryGetValue(id, out var evictedAt) && entry.LoadedAt <= evictedAt);

    private void PruneEvictions(long now)
    {
        var threshold = now - _evictionRetentionTicks;
        foreach (var (id, evictedAt) in _evictedAt)
        {
            if (evictedAt < threshold)
            {
                _ = _evictedAt.TryRemove(KeyValuePair.Create(id, evictedAt));
            }
        }
    }

    [ImmutableObject(true)]
    private sealed record TenantCacheEntry(Tenant? Tenant, long LoadedAt);
}
