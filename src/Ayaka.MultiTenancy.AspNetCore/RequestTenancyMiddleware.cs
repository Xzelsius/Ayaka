// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.MultiTenancy.AspNetCore;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

/// <summary>
///     Enables automatic tenant detection based on the current <see cref="HttpRequest"/>.
/// </summary>
public sealed partial class RequestTenancyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly RequestTenancyOptions _options;
    private readonly ITenantContextAccessor _accessor;
    private readonly ILogger _logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RequestTenancyMiddleware"/> class.
    /// </summary>
    /// <param name="next">The <see cref="RequestDelegate"/> representing the next middleware in the pipeline.</param>
    /// <param name="options">The <see cref="RequestTenancyOptions"/> representing the options for the middleware.</param>
    /// <param name="accessor">The <see cref="ITenantContextAccessor"/> used to store the detected tenant.</param>
    /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> used for logging.</param>
    public RequestTenancyMiddleware(
        RequestDelegate next,
        IOptions<RequestTenancyOptions> options,
        ITenantContextAccessor accessor,
        ILoggerFactory loggerFactory)
    {
        _next = next;
        _options = options.Value;
        _accessor = accessor;
        _logger = loggerFactory.CreateLogger<RequestTenancyMiddleware>();
    }

    /// <summary>
    ///     Invokes the logic of the middleware.
    /// </summary>
    /// <param name="context">The <see cref="HttpContext"/>.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint();

        // Check whether we should skip tenant detection
        if (endpoint?.Metadata.GetMetadata<DisableMultiTenancyAttribute>() is not null)
        {
            RequestTenancyLog.EndpointSkipsTenantDetection(_logger);
            return _next(context);
        }

        return InvokeInternal(context);
    }

    private async Task InvokeInternal(HttpContext context)
    {
        if (_accessor.TenantContext is not null)
        {
            throw new InvalidOperationException("Tenant was already set previously in the request pipeline");
        }

        if (_options.DetectionStrategies.Count == 0)
        {
            throw new InvalidOperationException("No tenant detection strategies are configured");
        }

        var tenant = await TryDetectTenant(context);
        if (tenant is not null)
        {
            var activityFeature = context.Features.Get<IHttpActivityFeature>();
            if (activityFeature is not null)
            {
                _ = activityFeature.Activity.SetTag(_options.ActivityTagName, tenant);
            }

            _accessor.TenantContext = new TenantContext(tenant, tenant);
        }

        await _next(context);
    }

    private async Task<string?> TryDetectTenant(HttpContext context)
    {
        foreach (var strategy in _options.DetectionStrategies)
        {
            var tenant = await strategy.TryDetectAsync(context);
            if (tenant is not null)
            {
                RequestTenancyLog.TenantDetected(_logger, tenant, strategy.GetType().Name);
                return tenant;
            }
        }

        RequestTenancyLog.UnableToDetectTenant(_logger);
        return null;
    }

    private static partial class RequestTenancyLog
    {
        [LoggerMessage(1, LogLevel.Debug, "Endpoint is marked to skip tenant detection.")]
        public static partial void EndpointSkipsTenantDetection(ILogger logger);

        [LoggerMessage(2, LogLevel.Debug, "Tenant '{Tenant}' was detected using strategy '{Strategy}'.")]
        public static partial void TenantDetected(ILogger logger, string tenant, string strategy);

        [LoggerMessage(3, LogLevel.Debug, "Unable to detect tenant.")]
        public static partial void UnableToDetectTenant(ILogger logger);
    }
}
