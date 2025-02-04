// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.MultiTenancy.AspNetCore.Detection;

using Microsoft.AspNetCore.Http;

/// <summary>
///     Represents a strategy for detecting the tenant from a request header.
/// </summary>
public sealed class FromRequestHeaderStrategy : ITenantDetectionStrategy
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="FromRequestHeaderStrategy"/> class.
    /// </summary>
    /// <param name="headerName">The name of the requests header to read the tenant from.</param>
    public FromRequestHeaderStrategy(string headerName)
    {
        HeaderName = headerName;
    }

    /// <summary>
    ///     Gets the name of the requests header to read the tenant from.
    /// </summary>
    public string HeaderName { get; }

    /// <inheritdoc />
    public Task<string?> TryDetectAsync(HttpContext context)
    {
        var headers = context.Request.Headers;

        if (!headers.TryGetValue(HeaderName, out var headerValues))
        {
            return Task.FromResult<string?>(null);
        }

        string? headerValue;

        // Catch possible multiple header values
        if (headerValues.Count > 1)
        {
            // Log.MultipleHeaderValuesFound(_logger, _headerName);
            headerValue = headerValues[0];
        }
        else
        {
            headerValue = headerValues;
        }

        // Catch possible empty or whitespace value
        if (string.IsNullOrWhiteSpace(headerValue))
        {
            return Task.FromResult<string?>(null);
        }

        var tenantId = headerValue;

        return Task.FromResult<string?>(tenantId);
    }
}
