// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.MultiTenancy.Management;

/// <summary>
///     Provides persistence functionality for <see cref="Tenant"/> instances.
/// </summary>
public interface ITenantStore
{
    /// <summary>
    ///     Adds the specified <paramref name="tenant"/> to the store.
    /// </summary>
    /// <param name="tenant">The tenant that should be added to the store.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task AddAsync(Tenant tenant, CancellationToken cancellationToken = default);

    /// <summary>
    ///    Updates the specified <paramref name="tenant"/> in the store.
    /// </summary>
    /// <param name="tenant">The tenant that should be updated in the store.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task UpdateAsync(Tenant tenant, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Removes the tenant with the specified <paramref name="id"/> from the store.
    /// </summary>
    /// <param name="id">The identifier of the tenant that should be removed from the store.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task RemoveAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets all tenants from the store.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns>A task that represents the asynchronous operation. The task results contains the tenants.</returns>
    Task<IReadOnlyList<Tenant>> GetAllAsync(CancellationToken cancellationToken = default);
}
