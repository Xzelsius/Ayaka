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
    public Task UpdateAsync(Tenant tenant, CancellationToken cancellationToken = default)
    {
        _ = _tenants.AddOrUpdate(
            tenant.Id,
            _ => throw new TenantManagementException($"Tenant with id '{tenant.Id}' does not exist"),
            (_, _) => tenant
        );

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<bool> RemoveAsync(string id, CancellationToken cancellationToken = default)
        => Task.FromResult(_tenants.TryRemove(id, out _));

    /// <inheritdoc />
    public Task<Tenant?> GetAsync(string id, CancellationToken cancellationToken = default)
        => Task.FromResult(_tenants.TryGetValue(id, out var tenant) ? tenant : null);

    /// <inheritdoc />
    public Task<IReadOnlyList<Tenant>> GetAllAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<Tenant>>([.._tenants.Values]);
}
