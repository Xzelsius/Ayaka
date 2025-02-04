// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.MultiTenancy.AspNetCore.Detection;

using Microsoft.AspNetCore.Http;

/// <summary>
///     Provides a strategy to detect the tenant based on the <see cref="HttpContext"/> of the current request.
/// </summary>
public interface ITenantDetectionStrategy
{
    /// <summary>
    ///     Tries to detect the tenant based on the <paramref name="context"/>.
    /// </summary>
    /// <remarks>
    ///     Instead of throwing an exception, this method should return <see langword="null"/> if the tenant could
    ///     not be detected. This allows another strategy to try to detect the tenant, if one is configured.
    /// </remarks>
    /// <param name="context">The <see cref="HttpContent"/> of the current request.</param>
    /// <returns>
    ///     A <see cref="Task{TResult}"/> that represents the asynchronous operation. The task result contains the tenant identifier,
    ///     if one could be detected; otherwise, <see langword="null"/>.
    /// </returns>
    Task<string?> TryDetectAsync(HttpContext context);
}
