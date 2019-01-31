// Copyright (c) Raphael Strotz and other contributors under the MIT license. All rights reserved.

using System.Threading;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace Ayaka.Caching.Memory
{
    public class MemoryCacheManagerTest
    {
        [Fact]
        public void Can_get_entry()
        {
            using (var cache = new MemoryCache(new MemoryCacheOptions()))
            {
                var obj = new object();
                cache.Set("obj", obj);

                var cacheManager = new MemoryCacheManager(cache, new CacheOptions());

                var fromCache = cacheManager.Get("obj");

                Assert.Equal(obj, fromCache);
            }
        }

        [Fact]
        public void Can_set_entry()
        {
            using (var cache = new MemoryCache(new MemoryCacheOptions()))
            {
                var obj = new object();

                var cacheManager = new MemoryCacheManager(cache, new CacheOptions());
                cacheManager.Set("obj", obj);

                Assert.Contains(cacheManager.Keys, key => key == "obj");

                var fromCache = cache.Get("obj");

                Assert.Equal(obj, fromCache);
            }
        }

        [Fact]
        public void Can_remove_entry()
        {
            using (var cache = new MemoryCache(new MemoryCacheOptions()))
            {
                var obj = new object();

                var cacheManager = new MemoryCacheManager(cache, new CacheOptions());
                cacheManager.Set("obj", obj);

                Assert.Contains(cacheManager.Keys, key => key == "obj");

                cacheManager.Remove("obj");

                Assert.DoesNotContain(cacheManager.Keys, key => key == "obj");
            }
        }

        [Fact]
        public void Can_clear_cache()
        {
            using (var cache = new MemoryCache(new MemoryCacheOptions()))
            {
                var obj1 = new object();
                var obj2 = new object();
                var obj3 = new object();

                var cacheManager = new MemoryCacheManager(cache, new CacheOptions());
                cacheManager.Set("obj1", obj1);
                cacheManager.Set("obj2", obj2);
                cacheManager.Set("obj3", obj3);

                Assert.NotEmpty(cacheManager.Keys);

                cacheManager.Clear();

                // A little hacky, there must be a better way
                // to clear cache and await post eviction
                Thread.Sleep(1000);

                Assert.Empty(cacheManager.Keys);
            }
        }

    }
}
