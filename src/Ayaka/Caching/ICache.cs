// Copyright (c) Raphael Strotz and other contributors under the MIT license. All rights reserved.

using System;
using System.Collections.Generic;

namespace Ayaka.Caching
{
    /// <summary>
    ///     Defines functionality to interact with caches.
    /// </summary>
    public interface ICache
    {
        /// <summary>
        ///     Gets an <see cref="IEnumerable{T}" /> containing the keys of all cached values.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}" /> containing the keys.</returns>
        IEnumerable<string> Keys { get; }

        /// <summary>
        ///     Gets a value indicating whether a value associated with the specified key exists or not.
        /// </summary>
        /// <param name="key">key</param>
        /// <returns>
        ///     <c>true</c> if a value under the specified key exists; otherwise <c>false</c>.
        /// </returns>
        bool Exists(string key);

        /// <summary>
        ///     Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <returns>The value associated with the specified key.</returns>
        object Get(string key);

        /// <summary>
        ///     Adds the specified key and value to the cache.
        /// </summary>
        /// <param name="key">The key of the value.</param>
        /// <param name="value">The value to store.</param>
        /// <param name="expiresIn">Optional expiration of the value.</param>
        void Set(string key, object value, TimeSpan? expiresIn = null);

        /// <summary>
        ///     Removes the value with the specified key from the cache.
        /// </summary>
        /// <param name="key">The key of the value.</param>
        void Remove(string key);

        /// <summary>
        ///     Clears all cached values.
        /// </summary>
        void Clear();
    }
}
