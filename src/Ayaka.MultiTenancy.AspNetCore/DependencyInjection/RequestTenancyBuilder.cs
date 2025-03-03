// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.MultiTenancy.DependencyInjection;

/// <summary>
///     A builder for configuring <see cref="RequestTenancyOptions"/> used by the <see cref="RequestTenancyMiddleware"/>.
/// </summary>
public sealed class RequestTenancyBuilder
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="RequestTenancyBuilder"/> class.
    /// </summary>
    /// <param name="multiTenancy">The <see cref="IMultiTenancyBuilder" /> to configure request tenancy on.</param>
    internal RequestTenancyBuilder(IMultiTenancyBuilder multiTenancy)
    {
        MultiTenancy = multiTenancy;
    }

    /// <summary>
    ///     Gets the <see cref="IMultiTenancyBuilder"/> where request tenancy is configured.
    /// </summary>
    public IMultiTenancyBuilder MultiTenancy { get; }

    internal void ConfigureServices()
    {
        // Ability to resolve IOptions<RequestTenancyOptions>
        var optionBuilder = MultiTenancy.Services.AddOptions<RequestTenancyOptions>();
    }
}
