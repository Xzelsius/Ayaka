// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.MultiTenancy.Management;

/// <summary>
///     Provides access to the storage for <see cref="Tenant"/> instances.
/// </summary>
public interface ITenantStorage
{
    /// <summary>
    ///     Adds the specified <paramref name="tenant"/> to the storage.
    /// </summary>
    /// <param name="tenant">The tenant that should be added to the storage.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task AddAsync(Tenant tenant, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Removes the tenant with the specified <paramref name="id"/> from the storage.
    /// </summary>
    /// <param name="id">The identifier of the tenant that should be removed from the storage.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task RemoveAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets all tenants from the storage.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns>A task that represents the asynchronous operation. The task results contains the tenants.</returns>
    Task<IReadOnlyList<Tenant>> GetAllAsync(CancellationToken cancellationToken = default);
}
