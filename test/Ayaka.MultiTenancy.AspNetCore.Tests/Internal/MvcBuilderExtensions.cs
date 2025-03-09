// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.MultiTenancy.AspNetCore.Tests.Internal;

using Microsoft.Extensions.DependencyInjection;

internal static class MvcBuilderExtensions
{
    /// <summary>
    ///     Adds the specified <paramref name="controllerTypes"/> to the available controllers.
    /// </summary>
    /// <remarks>
    ///     By default, ASP.NET Core does not recognize controllers that are private or nested inside another class.
    ///     This method allows adding controllers made for specific test cases to the available controllers.
    /// </remarks>
    /// <param name="mvcBuilder">The <see cref="IMvcBuilder"/> to add the controllers to.</param>
    /// <param name="controllerTypes">The controller types to add.</param>
    /// <returns>The same <see cref="IMvcBuilder"/>.</returns>
    public static IMvcBuilder AddTestControllers(this IMvcBuilder mvcBuilder, params Type[] controllerTypes)
    {
        mvcBuilder
            .PartManager
            .FeatureProviders
            .Add(new TestControllerFeatureProvider(controllerTypes));

        return mvcBuilder;
    }
}
