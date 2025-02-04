// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.MultiTenancy.AspNetCore.Detection;

using Microsoft.AspNetCore.Http;

/// <summary>
///     Represents a strategy for detecting the tenant from the request host.
/// </summary>
public sealed class FromRequestHostStrategy : ITenantDetectionStrategy
{
    /// <inheritdoc />
    public Task<string?> TryDetectAsync(HttpContext context)
    {
        if (!context.Request.Host.HasValue)
        {
            return Task.FromResult<string?>(null);
        }

        var host = context.Request.Host.Host;

        var parts = host.Split('.', StringSplitOptions.RemoveEmptyEntries);

        // localhost => 1 (e.g. not enough parts)
        // example.com => 2 (e.g. enough parts)
        // sub.example.com => 3 (e.g. enough parts)
        if (parts.Length <= 1)
        {
            return Task.FromResult<string?>(null);
        }

        // Use the left-most part as tenant identifier
        var tenantId = parts[0];

        return Task.FromResult<string?>(tenantId);
    }
}
