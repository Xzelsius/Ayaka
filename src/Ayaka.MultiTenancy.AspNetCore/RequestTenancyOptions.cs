// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.MultiTenancy.AspNetCore;

using Ayaka.MultiTenancy.AspNetCore.Detection;

/// <summary>
///     Represents the options to configure the behavior for the <see cref="RequestTenancyMiddleware"/>.
/// </summary>
public sealed class RequestTenancyOptions
{
    /// <summary>
    ///     Gets the configured tenant detection strategies.
    /// </summary>
    public IList<ITenantDetectionStrategy> DetectionStrategies { get; init; } = [];
}
