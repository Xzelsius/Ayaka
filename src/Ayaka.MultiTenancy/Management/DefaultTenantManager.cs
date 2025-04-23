// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.MultiTenancy.Management;

/// <summary>
///     The default <see cref="ITenantManager" /> providing basic tenant management capabilities
///     using an <see cref="ITenantStorage" /> as the underlying storage.
/// </summary>
internal sealed class DefaultTenantManager : ITenantManager
{
    private readonly ITenantStorage _storage;

    /// <summary>
    /// Initializes a new instance of <see cref="DefaultTenantManager"/>.
    /// </summary>
    /// <param name="storage">The underlying <see cref="ITenantStorage"/> to use.</param>
    public DefaultTenantManager(ITenantStorage storage)
    {
        _storage = storage;
    }

    /// <inheritdoc />
    public Task AddAsync(Tenant tenant, CancellationToken cancellationToken = default)
        => _storage.AddAsync(tenant, cancellationToken);

    /// <inheritdoc />
    public Task RemoveAsync(string id, CancellationToken cancellationToken = default)
        => _storage.RemoveAsync(id, cancellationToken);

    /// <inheritdoc />
    public async Task<Tenant?> GetAsync(string id, CancellationToken cancellationToken = default)
    {
        var allTenants = await _storage.GetAllAsync(cancellationToken);
        return allTenants.FirstOrDefault(t => t.Id == id);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<Tenant>> GetAllAsync(CancellationToken cancellationToken = default)
        => _storage.GetAllAsync(cancellationToken);
}
