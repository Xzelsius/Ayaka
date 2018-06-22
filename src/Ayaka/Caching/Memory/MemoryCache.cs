// Copyright (c) Raphael Strotz and other contributors under the MIT license. All rights reserved.

using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace Ayaka.Caching.Memory
{
    /// <summary>
    ///     Provides functionality to interact with a <see cref="IMemoryCache" />.
    /// </summary>
    public class MemoryCache : ICache
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IOptions<CacheOptions> _options;
        private readonly HashSet<string> _keys = new HashSet<string>();

        private CancellationTokenSource _tokenSource = new CancellationTokenSource();

        /// <summary>
        ///     Initializes a new instance of the <see cref="MemoryCache" /> class.
        /// </summary>
        /// <param name="memoryCache">The underlying <see cref="IMemoryCache" />.</param>
        /// <param name="options">THe cache options.</param>
        public MemoryCache(IMemoryCache memoryCache, IOptions<CacheOptions> options)
        {
            _memoryCache = memoryCache;
            _options = options;
        }

        /// <summary>
        ///     Gets an <see cref="IEnumerable{T}" /> containing the keys of all cached values.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}" /> containing the keys.</returns>
        public IEnumerable<string> Keys => _keys;

        /// <summary>
        ///     Gets a value indicating whether a value associated with the specified key exists or not.
        /// </summary>
        /// <param name="key">key</param>
        /// <returns>
        ///     <c>true</c> if a value under the specified key exists; otherwise <c>false</c>.
        /// </returns>
        public bool Exists(string key)
        {
            return _keys.Contains(key);
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
        /// <param name="expiresIn">The expiration of the value.</param>
        public void Set(string key, object value, TimeSpan? expiresIn)
        {
            var options = new MemoryCacheEntryOptions();
            options.SetAbsoluteExpiration(expiresIn ?? _options.Value.DefaultExpiration);
            options.AddExpirationToken(new CancellationChangeToken(_tokenSource.Token));
            options.RegisterPostEvictionCallback((o, k, r, s) =>
            {
                if (r == EvictionReason.Replaced) return;
                _keys.Remove(k.ToString());
            });

            _memoryCache.Set(key, value, options);
            _keys.Add(key);
        }

        /// <summary>
        ///     Removes the value with the specified key from the cache.
        /// </summary>
        /// <param name="key">The key of the value.</param>
        public void Remove(string key)
        {
            _memoryCache.Remove(key);
            _keys.Remove(key);
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

            _keys.Clear();
            _tokenSource = new CancellationTokenSource();
        }
    }
}
