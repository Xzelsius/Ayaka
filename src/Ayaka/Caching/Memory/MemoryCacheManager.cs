// Copyright (c) Raphael Strotz and other contributors under the MIT license. All rights reserved.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace Ayaka.Caching.Memory
{
    /// <summary>
    ///     Provides functionality to interact with a <see cref="IMemoryCache" />.
    /// </summary>
    public class MemoryCacheManager : ICacheManager
    {
        private readonly IMemoryCache _memoryCache;
        private readonly CacheOptions _options;
        private readonly ConcurrentDictionary<string, bool> _keys = new ConcurrentDictionary<string, bool>();

        private CancellationTokenSource _tokenSource = new CancellationTokenSource();

        /// <summary>
        ///     Initializes a new instance of the <see cref="MemoryCacheManager" /> class.
        /// </summary>
        /// <param name="memoryCache">The underlying <see cref="IMemoryCache" />.</param>
        /// <param name="optionsAccessor">The cache options.</param>
        public MemoryCacheManager(IMemoryCache memoryCache, IOptions<CacheOptions> optionsAccessor)
        {
            _memoryCache = memoryCache;
            _options = optionsAccessor.Value;
        }

        /// <summary>
        ///     Gets an <see cref="IEnumerable{T}" /> containing the keys of all cached values.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}" /> containing the keys.</returns>
        public IEnumerable<string> Keys 
            => _keys.Where(kvp => kvp.Value).Select(kvp => kvp.Key);

        /// <summary>
        ///     Gets a value indicating whether a value associated with the specified key exists or not.
        /// </summary>
        /// <param name="key">key</param>
        /// <returns>
        ///     <c>true</c> if a value under the specified key exists; otherwise <c>false</c>.
        /// </returns>
        public bool Exists(string key)
        {
            return _memoryCache.TryGetValue(key, out _);
        }

        /// <summary>
        ///     Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <returns>The value associated with the specified key.</returns>
        public object Get(string key)
        {
            return _memoryCache.Get(key);
        }

        /// <summary>
        ///     Adds the specified key and value to the cache.
        /// </summary>
        /// <param name="key">The key of the value.</param>
        /// <param name="value">The value to store.</param>
        /// <param name="expiresIn">Optional expiration of the value.</param> 
        /// <param name="expirationToken">Optional <see cref="IChangeToken"/> that causes the cache entry to expire.</param>
        public void Set(string key, object value, TimeSpan? expiresIn = null, IChangeToken expirationToken = null)
        {
            var options = new MemoryCacheEntryOptions();
            options.SetAbsoluteExpiration(expiresIn ?? _options.DefaultExpiration);
            options.AddExpirationToken(new CancellationChangeToken(_tokenSource.Token));

            if (expirationToken != null)
            {
                options.AddExpirationToken(expirationToken);
            }

            options.RegisterPostEvictionCallback((k, v, r, s) =>
            {
                if (r == EvictionReason.Replaced) return;

                CleanupKeys();
                TryRemoveKey(k.ToString());
            });

            _memoryCache.Set(key, value, options);
            _keys.TryAdd(key, value: true);
        }

        /// <summary>
        ///     Removes the value with the specified key from the cache.
        /// </summary>
        /// <param name="key">The key of the value.</param>
        public void Remove(string key)
        {
            _memoryCache.Remove(key);
            TryRemoveKey(key);
        }

        /// <summary>
        ///     Clears all cached values.
        /// </summary>
        public void Clear()
        {
            if (_tokenSource != null && !_tokenSource.IsCancellationRequested && _tokenSource.Token.CanBeCanceled)
            {
                _tokenSource.Cancel();
                _tokenSource.Dispose();
            }

            _tokenSource = new CancellationTokenSource();
        }

        /// <summary>
        ///     Removes the specified key from the internal key list.
        /// </summary>
        /// <param name="key">The key.</param>
        protected void TryRemoveKey(string key)
        {
            if (!_keys.TryRemove(key, out _))
            {
                _keys.TryUpdate(key, newValue: false, comparisonValue: true);
            }
        }

        /// <summary>
        ///     Clean flagged keys from the internal key list.
        /// </summary>
        protected void CleanupKeys()
        {
            var keys = _keys.Where(kvp => !kvp.Value).Select(kvp => kvp.Key);
            foreach (var key in keys)
            {
                TryRemoveKey(key);
            }
        }
    }
}
