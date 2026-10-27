// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.MultiTenancy.AspNetCore.Resolution;

using Ayaka.MultiTenancy.Management;

/// <summary>
///     Represents a tenant context resolver that resolves a <see cref="TenantContext"/> by looking up
///     the tenant in an <see cref="ITenantManager"/>.
/// </summary>
internal sealed class TenantManagementTenantContextResolver : ITenantContextResolver
{
    private readonly ITenantManager _manager;

    /// <summary>
    ///    Initializes a new instance of the <see cref="TenantManagementTenantContextResolver"/> class.
    /// </summary>
    /// <param name="manager">The <see cref="ITenantManager"/> to use when resolving tenant contexts.</param>
    public TenantManagementTenantContextResolver(ITenantManager manager)
    {
        _manager = manager;
    }

    // <inheritdoc/>
    public async ValueTask<TenantContext?> ResolveAsync(string id, CancellationToken cancellationToken)
    {
        var tenant = await _manager.GetAsync(id, cancellationToken);

        return tenant is null
            ? null
            : new TenantContext(tenant.Id, tenant.DisplayName);
    }
}
