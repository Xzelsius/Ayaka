// Copyright (c) Raphael Strotz and other contributors under the MIT license. All rights reserved.

using System;
using Microsoft.Extensions.Options;

namespace Ayaka.Caching
{
    /// <summary>
    ///     The base representation of the cache options.
    /// </summary>
    public class CacheOptions : IOptions<CacheOptions>
    {
        /// <summary>
        ///     Gets the default options.
        /// </summary>
        public CacheOptions Value => this;

        /// <summary>
        ///     Gets or sets the default lifetime of cache values.
        /// </summary>
        /// <value>A <see cref="TimeSpan" /> representing the lifetime.</value>
        public TimeSpan DefaultExpiration { get; set; } = TimeSpan.FromMinutes(60);
    }
}
