// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.MultiTenancy.Management;

using System.Collections.Concurrent;

/// <summary>
///     Represents in-memory persistence for <see cref="Tenant"/> instances.
/// </summary>
public sealed class InMemoryTenantStore : ITenantStore
{
    private readonly ConcurrentDictionary<string, Tenant> _tenants = new();

    /// <inheritdoc />
    public Task AddAsync(Tenant tenant, CancellationToken cancellationToken = default)
    {
        var added = _tenants.TryAdd(tenant.Id, tenant);
        if (!added)
        {
            throw new TenantManagementException($"Tenant with id '{tenant.Id}' already exists");
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task RemoveAsync(string id, CancellationToken cancellationToken = default)
    {
        _ = _tenants.TryRemove(id, out _);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<Tenant>> GetAllAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<Tenant>>([.._tenants.Values]);
}
