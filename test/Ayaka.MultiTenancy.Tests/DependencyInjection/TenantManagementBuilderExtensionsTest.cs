// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.MultiTenancy.Tests.DependencyInjection;

using Ayaka.MultiTenancy.DependencyInjection;
using Ayaka.MultiTenancy.Management;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

public sealed class TenantManagementBuilderExtensionsTest
{
    public sealed class UseInMemoryStore
    {
        [Fact]
        public void Does_return_same_instance_of_ITenantManagementBuilder()
        {
            var builder = new TestTenantManagementBuilder();

            var result = builder.UseInMemoryStore();

            result.ShouldBeSameAs(builder);
        }

        [Fact]
        public void Does_add_in_memory_tenant_store()
        {
            var builder = new TestTenantManagementBuilder();

            builder.UseInMemoryStore();

            var store = builder.MultiTenancy.Services.FirstOrDefault(x => x.ServiceType == typeof(ITenantStore));

            store.ShouldNotBeNull("ITenantStore should be registered");
            store.ImplementationType.ShouldBe(typeof(InMemoryTenantStore));
        }

        [Fact]
        public void Does_not_replace_user_registered_tenant_store()
        {
            var builder = new TestTenantManagementBuilder();
            builder.MultiTenancy.Services.AddSingleton<ITenantStore, TestTenantStore>();

            builder.UseInMemoryStore();

            var store = builder.MultiTenancy.Services.SingleOrDefault(x => x.ServiceType == typeof(ITenantStore));

            store.ShouldNotBeNull("ITenantStore should be registered");
            store.ImplementationType.ShouldBe(typeof(TestTenantStore));
        }
    }

    public sealed class UseHybridCache
    {
        [Fact]
        public void Does_return_same_instance_of_ITenantManagementBuilder()
        {
            var builder = new TestTenantManagementBuilder();

            var result = builder.UseHybridCache();

            result.ShouldBeSameAs(builder);
        }

        [Fact]
        public void Does_replace_the_default_tenant_cache()
        {
            var builder = new TestMultiTenancyBuilder().AddTenantManagement();

            builder.UseHybridCache();

            var cache = builder.MultiTenancy.Services.Where(x => x.ServiceType == typeof(ITenantCache)).ShouldHaveSingleItem();
            cache.ImplementationType.ShouldBe(typeof(HybridCacheTenantCache));
        }

        [Fact]
        public void Does_not_replace_user_registered_tenant_cache()
        {
            var builder = new TestMultiTenancyBuilder().AddTenantManagement();
            builder.MultiTenancy.Services.AddSingleton<ITenantCache, TestTenantCache>();

            builder.UseHybridCache();

            var cache = builder.MultiTenancy.Services.Where(x => x.ServiceType == typeof(ITenantCache)).ShouldHaveSingleItem();
            cache.ImplementationType.ShouldBe(typeof(TestTenantCache));
        }

        [Fact]
        public void Does_apply_the_configured_cache_options()
        {
            var builder = new TestMultiTenancyBuilder().AddTenantManagement();

            builder.UseHybridCache(options => options.Expiration = TimeSpan.FromMinutes(10));

            using var services = builder.MultiTenancy.Services.BuildServiceProvider();
            services.GetRequiredService<IOptions<TenantCacheOptions>>().Value.Expiration.ShouldBe(TimeSpan.FromMinutes(10));
        }
    }

    public sealed class UseKeyedHybridCache
    {
        [Fact]
        public void Does_return_same_instance_of_ITenantManagementBuilder()
        {
            var builder = new TestTenantManagementBuilder();

            var result = builder.UseKeyedHybridCache("tenancy");

            result.ShouldBeSameAs(builder);
        }

        [Fact]
        public void Does_replace_the_default_tenant_cache_with_a_keyed_hybrid_cache()
        {
            var builder = new TestMultiTenancyBuilder().AddTenantManagement();

            builder.UseKeyedHybridCache("tenancy");

            var cache = builder.MultiTenancy.Services.Where(x => x.ServiceType == typeof(ITenantCache)).ShouldHaveSingleItem();
            cache.ImplementationType.ShouldNotBe(typeof(NoopTenantCache));
        }

        [Fact]
        public void Does_resolve_the_tenant_cache_with_the_keyed_hybrid_cache()
        {
            var builder = new TestMultiTenancyBuilder().AddTenantManagement();
            builder.MultiTenancy.Services.AddKeyedHybridCache("tenancy");

            builder.UseKeyedHybridCache("tenancy");

            using var services = builder.MultiTenancy.Services.BuildServiceProvider();
            services.GetService<HybridCache>().ShouldBeNull("only the keyed HybridCache should be registered");
            services.GetRequiredService<ITenantCache>().ShouldBeOfType<HybridCacheTenantCache>();
        }

        [Fact]
        public void Does_fail_to_resolve_the_tenant_cache_when_the_keyed_hybrid_cache_is_missing()
        {
            var builder = new TestMultiTenancyBuilder().AddTenantManagement();

            builder.UseKeyedHybridCache("tenancy");

            using var services = builder.MultiTenancy.Services.BuildServiceProvider();

            Should.Throw<InvalidOperationException>(services.GetRequiredService<ITenantCache>);
        }

        [Fact]
        public void Does_reject_a_null_service_key()
        {
            Should.Throw<ArgumentNullException>(
                () => new TestTenantManagementBuilder().UseKeyedHybridCache(null!));
        }

        [Fact]
        public void Does_apply_the_configured_cache_options()
        {
            var builder = new TestMultiTenancyBuilder().AddTenantManagement();

            builder.UseKeyedHybridCache("tenancy", options => options.Expiration = TimeSpan.FromMinutes(10));

            using var services = builder.MultiTenancy.Services.BuildServiceProvider();
            services.GetRequiredService<IOptions<TenantCacheOptions>>().Value.Expiration.ShouldBe(TimeSpan.FromMinutes(10));
        }
    }

    private sealed class TestMultiTenancyBuilder : IMultiTenancyBuilder
    {
        public IServiceCollection Services { get; } = new ServiceCollection();
    }

    private sealed class TestTenantManagementBuilder : ITenantManagementBuilder
    {
        public IMultiTenancyBuilder MultiTenancy { get; } = new TestMultiTenancyBuilder();
    }

    private sealed class TestTenantStore : ITenantStore
    {
        public Task AddAsync(Tenant tenant, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task UpdateAsync(Tenant tenant, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<bool> RemoveAsync(string id, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<Tenant?> GetAsync(string id, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<IReadOnlyList<Tenant>> GetAllAsync(CancellationToken cancellationToken = default)
            => throw new NotImplementedException();
    }

    private sealed class TestTenantCache : ITenantCache
    {
        public ValueTask<Tenant?> GetOrCreateAsync(
            string id,
            Func<string, CancellationToken, ValueTask<Tenant?>> factory,
            CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public ValueTask EvictAsync(string id, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public ValueTask EvictAllAsync(CancellationToken cancellationToken = default)
            => throw new NotImplementedException();
    }
}
