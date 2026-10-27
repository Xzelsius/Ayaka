// Copyright (c) Raphael Strotz. All rights reserved.

#pragma warning disable IDE0130 // Namespaces does not match folder structure
namespace Ayaka.MultiTenancy.DependencyInjection;

using Ayaka.MultiTenancy.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

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
        VerifyServicesAreRegistered(app);
        return app.UseMiddleware<RequestTenancyMiddleware>();
    }

    private static void VerifyServicesAreRegistered(IApplicationBuilder app)
    {
        if (app.ApplicationServices.GetService<RequestTenancyMarkerService>() is null)
        {
            throw new InvalidOperationException(
                $"Unable to find the required services. " +
                $"Please add all the required services by calling '{nameof(IServiceCollection)}.AddMultiTenancy().ConfigureRequestTenancy(...)' in the application startup code.");
        }
    }
}
