﻿// Copyright (c) Raphael Strotz. All rights reserved.

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
    Task AddAsync(Tenant tenant, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Removes the tenant with the specified <paramref name="id"/> from the known tenants.
    /// </summary>
    /// <param name="id">The identifier of the tenant that should be removed.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task RemoveAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets the tenant with the specified <paramref name="id"/> identifier from the known tenants,
    ///     or <c>null</c> if the tenant does not exist.
    /// </summary>
    /// <param name="id">The identifier of the tenant to get.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains the tenant, if found; otherwise <c>null</c>.
    /// </returns>
    Task<Tenant?> GetAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets all known tenants.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the tenants.</returns>
    Task<IReadOnlyList<Tenant>> GetAllAsync(CancellationToken cancellationToken = default);
}
