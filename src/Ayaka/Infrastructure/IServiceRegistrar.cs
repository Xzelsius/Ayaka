// Copyright (c) Raphael Strotz and other contributors under the MIT license. All rights reserved.

using Microsoft.Extensions.DependencyInjection;

namespace Ayaka.Infrastructure
{
    /// <summary>
    ///     Defines functionality to add services to a service collection.
    /// </summary>
    public interface IServiceRegistrar
    {
        /// <summary>
        ///     Gets the call order of the registrar.
        /// </summary>
        /// <remarks>Lower number gets called first.</remarks>
        /// <value>An integer representing the order.</value>
        int Order { get; }

        /// <summary>
        ///     Adds services to the specified sevice collection.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
        void ConfigureServices(IServiceCollection services);
    }
}
