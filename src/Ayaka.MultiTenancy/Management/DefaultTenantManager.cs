// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.MultiTenancy.Management;

using Light.GuardClauses;

/// <summary>
///     The default <see cref="ITenantManager" /> providing basic tenant management capabilities
///     using an <see cref="ITenantStore" /> as the underlying persistence.
/// </summary>
/// <param name="store">The underlying <see cref="ITenantStore"/> to use.</param>
internal sealed class DefaultTenantManager(ITenantStore store) : ITenantManager
{
    /// <inheritdoc />
    public ValueTask AddAsync(Tenant tenant, CancellationToken cancellationToken = default)
        => new(store.AddAsync(tenant.MustNotBeNull(), cancellationToken));

    /// <inheritdoc />
    public ValueTask UpdateAsync(Tenant tenant, CancellationToken cancellationToken = default)
        => new(store.UpdateAsync(tenant.MustNotBeNull(), cancellationToken));

    /// <inheritdoc />
    public ValueTask<bool> RemoveAsync(string id, CancellationToken cancellationToken = default)
        => new(store.RemoveAsync(id.MustNotBeNullOrWhiteSpace(), cancellationToken));

    /// <inheritdoc />
    public ValueTask<Tenant?> GetAsync(string id, CancellationToken cancellationToken = default)
        => new(store.GetAsync(id.MustNotBeNullOrWhiteSpace(), cancellationToken));

    /// <inheritdoc />
    public ValueTask<IReadOnlyList<Tenant>> GetAllAsync(CancellationToken cancellationToken = default)
        => new(store.GetAllAsync(cancellationToken));
}
