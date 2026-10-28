// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.MultiTenancy.AspNetCore.Resolution;

/// <summary>
///     Represents a tenant context resolver that simply wraps the provided tenant ID as both
///     the ID and display name of the resulting <see cref="TenantContext"/>.
/// </summary>
internal sealed class DetectOnlyTenantContextResolver : ITenantContextResolver
{
    /// <inheritdoc />
    public ValueTask<TenantContext?> ResolveAsync(string id, CancellationToken cancellationToken)
        => ValueTask.FromResult<TenantContext?>(new TenantContext(id, id));
}
