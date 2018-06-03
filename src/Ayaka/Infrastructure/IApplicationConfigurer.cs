// Copyright (c) Raphael Strotz and other contributors under the MIT license. All rights reserved.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace Ayaka.Infrastructure
{
    /// <summary>
    ///     Defines functionality to apply configuration to an application builder.
    /// </summary>
    public interface IApplicationConfigurer
    {
        /// <summary>
        ///     Gets the call order of the configurer.
        /// </summary>
        /// <remarks>Lower number gets called first.</remarks>
        /// <value>An integer representing the order.</value>
        int Order { get; }

        /// <summary>
        ///     Applies configuration to the specified application builder.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder" />.</param>
        /// <param name="env">An instance of <see cref="IHostingEnvironment" />.</param>
        void Configure(IApplicationBuilder app, IHostingEnvironment env);
    }
}
