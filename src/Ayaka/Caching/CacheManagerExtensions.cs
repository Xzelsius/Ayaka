// Copyright (c) Raphael Strotz and other contributors under the MIT license. All rights reserved.

using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;

namespace Ayaka.Caching
{
    /// <summary>
    ///     Provides additional functionality for <see cref="ICacheManager" />.
    /// </summary>
    public static class CacheManagerExtensions
    {
        /// <summary>
        ///     Gets the value associated with the specified key.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="cacheManager">The cache manager instance.</param>
        /// <param name="key">The key of the value to get.</param>
        /// <returns>The value associated with the specified key.</returns>
        public static TValue Get<TValue>(this ICacheManager cacheManager, string key)
        {
            if (cacheManager == null) throw new ArgumentNullException(nameof(cacheManager));

            return (TValue) cacheManager.Get(key);
        }

        /// <summary>
        ///     Gets the value associated with the specified key.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="cacheManager">The cache manager instance.</param>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="acquire">The function to acquire the cache value, if it does not exist.</param>
        /// <param name="expiresIn">Optional expiration of the value.</param>
        /// <param name="expirationToken">Optional <see cref="IChangeToken"/> that causes the cache entry to expire.</param>
        /// <returns>The value associated with the specified key.</returns>
        public static TValue Get<TValue>(this ICacheManager cacheManager, string key, Func<TValue> acquire, TimeSpan? expiresIn = null, IChangeToken expirationToken = null)
        {
            if (cacheManager == null) throw new ArgumentNullException(nameof(cacheManager));

            if (cacheManager.Exists(key)) return (TValue) cacheManager.Get(key);

            var value = acquire();
            cacheManager.Set(key, value, expiresIn, expirationToken);

            return value;
        }

        /// <summary>
        ///     Gets the value associated with the specified key asynchronously.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="cacheManager">The cache manager instance.</param>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="acquire">The function to acquire the cache value, if it does not exist.</param>
        /// <param name="expiresIn">Optional expiration of the value.</param>
        /// <param name="expirationToken">Optional <see cref="IChangeToken"/> that causes the cache entry to expire.</param>
        /// <returns>
        ///     A task that represents the asynchronous get operation. The task result contains the value associated with the
        ///     specified key.
        /// </returns>
        public static async Task<TValue> GetAsync<TValue>(this ICacheManager cacheManager, string key, Func<Task<TValue>> acquire, TimeSpan? expiresIn = null, IChangeToken expirationToken = null)
        {
            if (cacheManager == null) throw new ArgumentNullException(nameof(cacheManager));

            if (cacheManager.Exists(key)) return (TValue) cacheManager.Get(key);

            var value = await acquire();
            cacheManager.Set(key, value, expiresIn, expirationToken);

            return value;
        }

        /// <summary>
        ///     Adds the specified key and value to the cache.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="cacheManager">The cache manager instance.</param>
        /// <param name="key">The key of the value.</param>
        /// <param name="value">The value to store.</param>
        /// <param name="expiresIn">Optional expiration of the value.</param>
        /// <param name="expirationToken">Optional <see cref="IChangeToken"/> that causes the cache entry to expire.</param>
        public static void Set<TValue>(this ICacheManager cacheManager, string key, TValue value, TimeSpan? expiresIn = null, IChangeToken expirationToken = null)
        {
            if (cacheManager == null) throw new ArgumentNullException(nameof(cacheManager));

            cacheManager.Set(key, value, expiresIn, expirationToken);
        }

        /// <summary>
        ///     Removes all values matching with the specified pattern from the cache.
        /// </summary>
        /// <param name="cacheManager">The cache manager instance.</param>
        /// <param name="pattern">The regular expression.</param>
        public static void RemoveByPattern(this ICacheManager cacheManager, string pattern)
        {
            if (cacheManager == null) throw new ArgumentNullException(nameof(cacheManager));
            if (pattern == null) throw new ArgumentNullException(nameof(pattern));

            var regex = new Regex(pattern, RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var keys = cacheManager.Keys.Where(k => regex.IsMatch(k));

            foreach (var key in keys)
            {
                cacheManager.Remove(key);
            }
        }
    }
}