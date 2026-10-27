// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.MultiTenancy.AspNetCore;

using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Ayaka.MultiTenancy.AspNetCore.Detection;
using Ayaka.MultiTenancy.AspNetCore.Resolution;
using Microsoft.AspNetCore.Http.Features;

/// <summary>
///     Represents the options to configure the behavior for the <see cref="RequestTenancyMiddleware"/>.
/// </summary>
public sealed class RequestTenancyOptions
{
    /// <summary>
    ///     Gets the configured tenant detection strategies.
    /// </summary>
    [Required]
    [MinLength(1)]
    public IList<ITenantDetectionStrategy> DetectionStrategies { get; init; } = [];

    /// <summary>
    ///     Gets or sets the resolver used to obtain the tenant context from a detected tenant identifier.
    /// </summary>
    [Required]
    public ITenantContextResolver ContextResolver { get; internal set; } = new DetectOnlyTenantContextResolver();

    /// <summary>
    ///     Gets or sets the name of the tag written to the request's <see cref="Activity"/> after the middleware
    ///     successfully detected a tenant.
    /// </summary>
    /// <remarks>
    ///     The tag is only written if the <see cref="IHttpActivityFeature"/> is available. It does not use
    ///     <see cref="Activity.Current"/>.
    /// </remarks>
    public string ActivityTagName { get; set; } = "tenant";
}
