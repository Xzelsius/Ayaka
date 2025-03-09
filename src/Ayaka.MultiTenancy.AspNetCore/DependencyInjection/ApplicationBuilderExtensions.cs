// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.MultiTenancy.DependencyInjection;

using Ayaka.MultiTenancy.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

/// <summary>
///     Extension methods for <see cref="IApplicationBuilder"/>.
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    ///     Adds the <see cref="RequestTenancyMiddleware"/> to automatically detect the tenant based on the request.
    /// </summary>
    /// <param name="app">The <see cref="IApplicationBuilder"/> to add the middleware to.</param>
    /// <returns>The same <see cref="IApplicationBuilder"/>.</returns>
    public static IApplicationBuilder UseMultiTenancy(this IApplicationBuilder app)
    {
        VerifyServiceAreRegistered(app);
        return app.UseMiddleware<RequestTenancyMiddleware>();
    }

    private static void VerifyServiceAreRegistered(IApplicationBuilder app)
    {
        var serviceProviderIsService = app.ApplicationServices.GetService<IServiceProviderIsService>();
        if (serviceProviderIsService != null && !serviceProviderIsService.IsService(typeof(ITenantContextAccessor)))
        {
            throw new InvalidOperationException(
                $"Unable to find the required services. " +
                $"Please add all the required services by calling '{nameof(IServiceCollection)}.AddMultiTenancy()' in the application startup code.");
        }
    }
}
