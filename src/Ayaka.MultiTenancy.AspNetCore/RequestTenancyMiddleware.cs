// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.MultiTenancy.AspNetCore;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
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
        if (endpoint is null)
        {
            RequestTenancyLog.NoEndpointMatched(_logger);
            return _next(context);
        }

        // Check whether we should skip tenant detection
        if (endpoint.Metadata.GetMetadata<DisableMultiTenancyAttribute>() is not null)
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

        var id = await TryDetectTenant(context);
        if (id is null)
        {
            await RejectAsync(context, StatusCodes.Status400BadRequest, "Tenant could not be detected.");
            return;
        }

        var tenantContext = await TryResolveTenant(id, context);
        if (tenantContext is null)
        {
            await RejectAsync(context, StatusCodes.Status404NotFound, "Tenant is unknown.");
            return;
        }

        var activityFeature = context.Features.Get<IHttpActivityFeature>();
        if (activityFeature is not null)
        {
            activityFeature.Activity.SetTag(_options.ActivityTagName, tenantContext.Id);
        }

        _accessor.TenantContext = tenantContext;
        try
        {
            await _next(context);
        }
        finally
        {
            _accessor.TenantContext = null;
        }
    }

    private static async Task RejectAsync(HttpContext context, int statusCode, string title)
    {
        context.Response.StatusCode = statusCode;

        var problemDetails = context.RequestServices.GetService<IProblemDetailsService>();
        if (problemDetails is not null)
        {
            await problemDetails.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = context,
                ProblemDetails =
                {
                    Status = statusCode,
                    Title = title
                },
            });
        }
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

    private async Task<TenantContext?> TryResolveTenant(string id, HttpContext context)
    {
        var tenantContext = await _options.ContextResolver.ResolveAsync(id, context.RequestAborted);
        if (tenantContext is not null)
        {
            RequestTenancyLog.TenantFound(_logger, tenantContext.Id);
            return tenantContext;
        }

        RequestTenancyLog.UnknownTenant(_logger, id);
        return null;

    }

    private static partial class RequestTenancyLog
    {
        [LoggerMessage(1, LogLevel.Debug, "No endpoint matched. Skipping tenant detection.")]
        public static partial void NoEndpointMatched(ILogger logger);

        [LoggerMessage(2, LogLevel.Debug, "Endpoint is marked to skip tenant detection.")]
        public static partial void EndpointSkipsTenantDetection(ILogger logger);

        [LoggerMessage(3, LogLevel.Debug, "Tenant '{Tenant}' was detected using strategy '{Strategy}'.")]
        public static partial void TenantDetected(ILogger logger, string tenant, string strategy);

        [LoggerMessage(4, LogLevel.Debug, "Unable to detect tenant.")]
        public static partial void UnableToDetectTenant(ILogger logger);

        [LoggerMessage(5, LogLevel.Debug, "Detected tenant {Tenant} was found.")]
        public static partial void TenantFound(ILogger logger, string tenant);

        [LoggerMessage(6, LogLevel.Debug, "Detected tenant {Tenant} is unknown.")]
        public static partial void UnknownTenant(ILogger logger, string tenant);
    }
}
