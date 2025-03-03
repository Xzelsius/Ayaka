// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.MultiTenancy.DependencyInjection;

using Ayaka.MultiTenancy.AspNetCore;

/// <summary>
///     Extension methods for <see cref="IMultiTenancyBuilder" />.
/// </summary>
public static class MultiTenancyBuilderExtensions
{
    /// <summary>
    ///     Configures the <see cref="RequestTenancyMiddleware"/> middleware.
    /// </summary>
    /// <param name="builder">The <see cref="IMultiTenancyBuilder"/> to configure the request tenancy on.</param>
    /// <param name="configure">A delegate to configure the <see cref="RequestTenancyOptions"/>.</param>
    /// <returns>The same <see cref="IMultiTenancyBuilder"/>.</returns>
    public static IMultiTenancyBuilder ConfigureRequestTenancy(this IMultiTenancyBuilder builder, Action<RequestTenancyBuilder> configure)
    {
        var requestTenancyBuilder = new RequestTenancyBuilder(builder);
        configure(requestTenancyBuilder);

        requestTenancyBuilder.ConfigureServices();

        return builder;
    }
}
