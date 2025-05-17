// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.MultiTenancy.Management;

/// <summary>
///     The default <see cref="ITenantManager" /> providing basic tenant management capabilities
///     using an <see cref="ITenantStore" /> as the underlying persistence.
/// </summary>
internal sealed class DefaultTenantManager : ITenantManager
{
    private readonly ITenantStore _store;

    /// <summary>
    /// Initializes a new instance of <see cref="DefaultTenantManager"/>.
    /// </summary>
    /// <param name="store">The underlying <see cref="ITenantStore"/> to use.</param>
    public DefaultTenantManager(ITenantStore store)
    {
        _store = store;
    }

    /// <inheritdoc />
    public Task AddAsync(Tenant tenant, CancellationToken cancellationToken = default)
        => _store.AddAsync(tenant, cancellationToken);

    /// <inheritdoc />
    public Task RemoveAsync(string id, CancellationToken cancellationToken = default)
        => _store.RemoveAsync(id, cancellationToken);

    /// <inheritdoc />
    public async Task<Tenant?> GetAsync(string id, CancellationToken cancellationToken = default)
    {
        var allTenants = await _store.GetAllAsync(cancellationToken);
        return allTenants.FirstOrDefault(t => t.Id == id);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<Tenant>> GetAllAsync(CancellationToken cancellationToken = default)
        => _store.GetAllAsync(cancellationToken);
}
