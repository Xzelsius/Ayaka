// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.MultiTenancy.AspNetCore.Resolution;

/// <summary>
///     Resolves a detected tenant identifier to the context used for the current request.
/// </summary>
public interface ITenantContextResolver
{
    /// <summary>
    ///     Resolves the specified <paramref name="id"/> to a tenant context.
    /// </summary>
    /// <param name="id">The detected tenant identifier.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>The tenant context, or <see langword="null"/> if the tenant is unknown.</returns>
    ValueTask<TenantContext?> ResolveAsync(string id, CancellationToken cancellationToken);
}
