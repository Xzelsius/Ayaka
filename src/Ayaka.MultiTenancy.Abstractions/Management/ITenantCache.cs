// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.MultiTenancy.Management;

/// <summary>
///     Provides caching functionality for <see cref="Tenant"/> instances.
/// </summary>
public interface ITenantCache
{
    /// <summary>
    ///     Gets the tenant with the specified <paramref name="id"/> from the cache, creating the entry when it does not exist.
    /// </summary>
    /// <param name="id">The identifier of the tenant to get.</param>
    /// <param name="factory">The factory used to create a missing cache entry.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains the tenant, if found;
    ///     otherwise, <see langword="null"/>. A <see langword="null"/> result is cached.
    /// </returns>
    ValueTask<Tenant?> GetOrCreateAsync(
        string id,
        Func<string, CancellationToken, ValueTask<Tenant?>> factory,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Evicts the cached tenant with the specified <paramref name="id"/>.
    /// </summary>
    /// <param name="id">The identifier of the tenant to evict.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    ValueTask EvictAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Evicts all cached tenants.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    ValueTask EvictAllAsync(CancellationToken cancellationToken = default);
}
