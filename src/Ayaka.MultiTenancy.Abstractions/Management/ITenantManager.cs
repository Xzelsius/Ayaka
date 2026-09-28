// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.MultiTenancy.Management;

/// <summary>
///     Provides tenant management capabilities.
/// </summary>
public interface ITenantManager
{
    /// <summary>
    ///     Adds the specified <paramref name="tenant"/> to the known tenants.
    /// </summary>
    /// <param name="tenant">The tenant that should be added.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    ValueTask AddAsync(Tenant tenant, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Updates the specified <paramref name="tenant"/> in the known tenants.
    /// </summary>
    /// <param name="tenant">The tenant that should be updated.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    ValueTask UpdateAsync(Tenant tenant, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Removes the tenant with the specified <paramref name="id"/> from the known tenants.
    /// </summary>
    /// <param name="id">The identifier of the tenant that should be removed.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result is <see langword="true"/> if a tenant
    ///     was removed; otherwise, <see langword="false"/>.
    /// </returns>
    ValueTask<bool> RemoveAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets the tenant with the specified <paramref name="id"/> identifier from the known tenants.
    /// </summary>
    /// <param name="id">The identifier of the tenant to get.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains the tenant, if found;
    ///     otherwise, <see langword="null"/>.
    /// </returns>
    ValueTask<Tenant?> GetAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets all known tenants.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the tenants.</returns>
    ValueTask<IReadOnlyList<Tenant>> GetAllAsync(CancellationToken cancellationToken = default);
}
