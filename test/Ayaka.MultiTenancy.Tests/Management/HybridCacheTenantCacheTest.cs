// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.MultiTenancy.Tests.Management;

using Ayaka.MultiTenancy.Management;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;

public sealed class HybridCacheFixture : TenantCacheFixture
{
    private readonly ServiceProvider _services;

    public HybridCacheFixture()
    {
        var services = new ServiceCollection();
        services.AddHybridCache();
        services.AddOptions<HybridCacheTenantCacheOptions>();
        services.AddSingleton<ITenantCache, HybridCacheTenantCache>();

        _services = services.BuildServiceProvider();

        Cache = _services.GetRequiredService<ITenantCache>();
    }

    public override ITenantCache Cache { get; }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _services.Dispose();
        }
    }

    protected override ValueTask DisposeAsyncCore()
        => _services.DisposeAsync();
}

public sealed class HybridCacheTenantCacheTest : TenantCacheCompliance<HybridCacheFixture>
{
    [Fact]
    public async Task Returns_the_same_tenant_reference_from_the_memory_cache()
    {
        var tenant = new Tenant("tenant1");

        var first = await CacheFixture.Cache.GetOrCreateAsync(
            tenant.Id,
            (_, _) => ValueTask.FromResult<Tenant?>(tenant),
            TestContext.Current.CancellationToken);
        var second = await CacheFixture.Cache.GetOrCreateAsync(
            tenant.Id,
            (_, _) => ValueTask.FromResult<Tenant?>(null),
            TestContext.Current.CancellationToken);

        first.ShouldBeSameAs(tenant);
        second.ShouldBeSameAs(tenant);
    }

    [Fact]
    public async Task Reloads_a_tenant_that_was_evicted_while_it_was_loading()
    {
        var (services, timeProvider, cache) = BuildWithFakeTiming();
        await using (services)
        {
            var loads = 0;

            var result = await cache.GetOrCreateAsync(
                "tenant1",
                async (id, ct) =>
                {
                    if (++loads == 1)
                    {
                        timeProvider.Advance(TimeSpan.FromSeconds(1));
                        await cache.EvictAsync(id, ct);
                        timeProvider.Advance(TimeSpan.FromSeconds(1));
                    }

                    return new Tenant(id)
                    {
                        DisplayName = $"load {loads}"
                    };
                },
                TestContext.Current.CancellationToken);

            loads.ShouldBe(2);
            result.ShouldNotBeNull().DisplayName.ShouldBe("load 2");
        }
    }

    [Fact]
    public async Task Does_not_reload_a_tenant_loaded_after_its_eviction()
    {
        var (services, timeProvider, cache) = BuildWithFakeTiming();
        await using (services)
        {
            var loads = 0;

            await cache.EvictAsync("tenant1", TestContext.Current.CancellationToken);
            timeProvider.Advance(TimeSpan.FromSeconds(1));

            await cache.GetOrCreateAsync(
                "tenant1",
                (id, _) =>
                {
                    loads++;
                    return ValueTask.FromResult<Tenant?>(new Tenant(id));
                },
                TestContext.Current.CancellationToken);

            loads.ShouldBe(1);
        }
    }

    [Theory]
    [InlineData(null, "billing-api")]
    [InlineData("billing-api", "orders.api")]
    public async Task Does_not_share_tenants_between_different_key_prefixes(string? firstPrefix, string? secondPrefix)
    {
        await using var services = BuildSharedHybridCache();
        var first = CreateCache(services, firstPrefix);
        var second = CreateCache(services, secondPrefix);

        await first.GetOrCreateAsync(
            "tenant1",
            (id, _) => ValueTask.FromResult<Tenant?>(new Tenant(id)),
            TestContext.Current.CancellationToken);
        var result = await second.GetOrCreateAsync(
            "tenant1",
            (_, _) => ValueTask.FromResult<Tenant?>(null),
            TestContext.Current.CancellationToken);

        result.ShouldBeNull();
    }

    [Fact]
    public async Task Does_not_share_tenants_between_no_key_prefix_and_a_key_prefix_that_looks_like_a_tenant_id()
    {
        await using var services = BuildSharedHybridCache();
        var unprefixed = CreateCache(services, null);
        var prefixed = CreateCache(services, "tenant");

        await unprefixed.GetOrCreateAsync(
            "tenant:tenant1",
            (id, _) => ValueTask.FromResult<Tenant?>(new Tenant(id)),
            TestContext.Current.CancellationToken);
        var result = await prefixed.GetOrCreateAsync(
            "tenant1",
            (_, _) => ValueTask.FromResult<Tenant?>(null),
            TestContext.Current.CancellationToken);

        result.ShouldBeNull();
    }

    [Fact]
    public async Task Shares_tenants_between_caches_with_the_same_key_prefix()
    {
        await using var services = BuildSharedHybridCache();
        var first = CreateCache(services, "billing-api");
        var second = CreateCache(services, "billing-api");
        var tenant = new Tenant("tenant1");

        await first.GetOrCreateAsync(
            tenant.Id,
            (_, _) => ValueTask.FromResult<Tenant?>(tenant),
            TestContext.Current.CancellationToken);
        var result = await second.GetOrCreateAsync(
            tenant.Id,
            (_, _) => ValueTask.FromResult<Tenant?>(null),
            TestContext.Current.CancellationToken);

        result.ShouldBe(tenant);
    }

    [Fact]
    public async Task Evicting_a_tenant_keeps_the_tenant_of_another_key_prefix()
    {
        await using var services = BuildSharedHybridCache();
        var first = CreateCache(services, "billing-api");
        var second = CreateCache(services, "orders-api");
        var tenant = new Tenant("tenant1");

        await second.GetOrCreateAsync(
            tenant.Id,
            (_, _) => ValueTask.FromResult<Tenant?>(tenant),
            TestContext.Current.CancellationToken);
        await first.EvictAsync(tenant.Id, TestContext.Current.CancellationToken);
        var result = await second.GetOrCreateAsync(
            tenant.Id,
            (_, _) => ValueTask.FromResult<Tenant?>(null),
            TestContext.Current.CancellationToken);

        result.ShouldBe(tenant);
    }

    [Fact]
    public async Task Evicting_all_tenants_keeps_the_tenants_of_another_key_prefix()
    {
        await using var services = BuildSharedHybridCache();
        var first = CreateCache(services, "billing-api");
        var second = CreateCache(services, "orders-api");
        var tenant = new Tenant("tenant1");

        await second.GetOrCreateAsync(
            tenant.Id,
            (_, _) => ValueTask.FromResult<Tenant?>(tenant),
            TestContext.Current.CancellationToken);
        await first.EvictAllAsync(TestContext.Current.CancellationToken);
        var result = await second.GetOrCreateAsync(
            tenant.Id,
            (_, _) => ValueTask.FromResult<Tenant?>(null),
            TestContext.Current.CancellationToken);

        result.ShouldBe(tenant);
    }

    private static ServiceProvider BuildSharedHybridCache()
    {
        var services = new ServiceCollection();
        services.AddHybridCache();

        return services.BuildServiceProvider();
    }

    private static HybridCacheTenantCache CreateCache(ServiceProvider services, string? keyPrefix)
        => new(
            services.GetRequiredService<HybridCache>(),
            Options.Create(new HybridCacheTenantCacheOptions { KeyPrefix = keyPrefix }),
            TimeProvider.System);

    private static (ServiceProvider, FakeTimeProvider, ITenantCache) BuildWithFakeTiming()
    {
        var timeProvider = new FakeTimeProvider();
        var services = new ServiceCollection();

        services.AddSingleton<TimeProvider>(timeProvider);
        services.AddHybridCache();
        services.AddOptions<HybridCacheTenantCacheOptions>();
        services.AddSingleton<ITenantCache, HybridCacheTenantCache>();

        var serviceProvider = services.BuildServiceProvider();

        return (serviceProvider, timeProvider, serviceProvider.GetRequiredService<ITenantCache>());
    }
}
