// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.MultiTenancy.Management;

/// <summary>
///     An <see cref="ITenantCache"/> that does not cache, always loading tenants through the factory.
/// </summary>
internal sealed class NoopTenantCache : ITenantCache
{
    /// <inheritdoc />
    public ValueTask<Tenant?> GetOrCreateAsync(
        string id,
        Func<string, CancellationToken, ValueTask<Tenant?>> factory,
        CancellationToken cancellationToken = default)
        => factory(id, cancellationToken);

    /// <inheritdoc />
    public ValueTask EvictAsync(string id, CancellationToken cancellationToken = default)
        => ValueTask.CompletedTask;

    /// <inheritdoc />
    public ValueTask EvictAllAsync(CancellationToken cancellationToken = default)
        => ValueTask.CompletedTask;
}
