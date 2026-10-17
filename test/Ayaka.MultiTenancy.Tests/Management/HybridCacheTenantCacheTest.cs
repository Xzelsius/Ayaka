// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.MultiTenancy.Tests.Management;

using Ayaka.MultiTenancy.Management;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;

public sealed class HybridCacheFixture : TenantCacheFixture
{
    private readonly ServiceProvider _services;

    public HybridCacheFixture()
    {
        var services = new ServiceCollection();
        services.AddHybridCache();
        services.AddOptions<TenantCacheOptions>();
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

    private static (ServiceProvider, FakeTimeProvider, ITenantCache) BuildWithFakeTiming()
    {
        var timeProvider = new FakeTimeProvider();
        var services = new ServiceCollection();

        services.AddSingleton<TimeProvider>(timeProvider);
        services.AddHybridCache();
        services.AddOptions<TenantCacheOptions>();
        services.AddSingleton<ITenantCache, HybridCacheTenantCache>();

        var serviceProvider = services.BuildServiceProvider();

        return (serviceProvider, timeProvider, serviceProvider.GetRequiredService<ITenantCache>());
    }
}
