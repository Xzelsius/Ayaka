// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.MultiTenancy.Tests.Management;

using Ayaka.MultiTenancy.Management;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;

public sealed class HybridCacheFixture : TenantCacheFixture
{
    private readonly ServiceProvider _services;

    public HybridCacheFixture()
    {
        var services = new ServiceCollection();
        _ = services.AddHybridCache();
        _ = services.AddOptions<TenantCacheOptions>();
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
}
