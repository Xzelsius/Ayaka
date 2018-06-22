// Copyright (c) Raphael Strotz and other contributors under the MIT license. All rights reserved.

using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Ayaka.Caching
{
    /// <summary>
    ///     Provides additional functionality for <see cref="ICache" />.
    /// </summary>
    public static class CacheExtensions
    {
        /// <summary>
        ///     Gets the value associated with the specified key.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="cache">The cache instance.</param>
        /// <param name="key">The key of the value to get.</param>
        /// <returns>The value associated with the specified key.</returns>
        public static TValue Get<TValue>(this ICache cache, string key)
        {
            if (cache == null) throw new ArgumentNullException(nameof(cache));

            return (TValue) cache.Get(key);
        }

        /// <summary>
        ///     Gets the value associated with the specified key.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="cache">The cache instance.</param>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="acquire">The function to acquire the cache value, if it does not exist.</param> 
        /// <param name="expiresIn">The expiration of the value.</param>
        /// <returns>The value associated with the specified key.</returns>
        public static TValue Get<TValue>(this ICache cache, string key, Func<TValue> acquire, TimeSpan? expiresIn)
        {
            if (cache == null) throw new ArgumentNullException(nameof(cache));

            if (cache.Exists(key)) return (TValue) cache.Get(key);

            var value = acquire();
            cache.Set(key, value, expiresIn);

            return value;
        }

        /// <summary>
        ///     Adds the specified key and value to the cache.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="cache">The cache instance.</param>
        /// <param name="key">The key of the value.</param>
        /// <param name="value">The value to store.</param> 
        /// <param name="expiresIn">The expiration of the value.</param>
        public static void Set<TValue>(this ICache cache, string key, TValue value, TimeSpan? expiresIn)
        {
            if (cache == null) throw new ArgumentNullException(nameof(cache));

            cache.Set(key, value, expiresIn);
        }

        /// <summary>
        ///     Removes all values matching with the specified pattern from the cache.
        /// </summary>
        /// <param name="cache">The cache instance.</param>
        /// <param name="pattern">The regular expression.</param>
        public static void RemoveByPattern(this ICache cache, string pattern)
        {
            if (cache == null) throw new ArgumentNullException(nameof(cache));
            if (pattern == null) throw new ArgumentNullException(nameof(pattern));

            var regex = new Regex(pattern, RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var keys = cache.Keys.Where(k => regex.IsMatch(k));

            foreach (var key in keys)
            {
                cache.Remove(key);
            }
        }
    }
}
