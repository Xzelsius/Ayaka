// Copyright (c) Raphael Strotz and other contributors under the MIT license. All rights reserved.

using Microsoft.AspNetCore.Routing;

namespace Ayaka.Routing
{
    /// <summary>
    ///     Defines functionality to add routes to a route collection.
    /// </summary>
    public interface IRouteRegistrar
    {
        /// <summary>
        ///     Gets the call order of the registrar.
        /// </summary>
        /// <remarks>Lower number gets called first.</remarks>
        /// <value>An integer representing the order.</value>
        int Order { get; }

        /// <summary>
        ///     Applies configurations to route builder.
        /// </summary>
        /// <param name="builder">The <see cref="IRouteBuilder" />.</param>
        void ConfigureRoutes(IRouteBuilder builder);
    }
}
