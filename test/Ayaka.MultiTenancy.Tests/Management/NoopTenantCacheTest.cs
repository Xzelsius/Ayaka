// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.MultiTenancy.Tests.Management;

using Ayaka.MultiTenancy.Management;

public sealed class NoopTenantCacheTest
{
    [Fact]
    public async Task Does_load_the_tenant_through_the_factory_on_every_call()
    {
        var cache = new NoopTenantCache();
        var tenant = new Tenant("tenant1");
        var factoryCalls = 0;

        ValueTask<Tenant?> Factory(string _, CancellationToken __)
        {
            factoryCalls++;
            return ValueTask.FromResult<Tenant?>(tenant);
        }

        var first = await cache.GetOrCreateAsync(tenant.Id, Factory, TestContext.Current.CancellationToken);
        var second = await cache.GetOrCreateAsync(tenant.Id, Factory, TestContext.Current.CancellationToken);

        first.ShouldBeSameAs(tenant);
        second.ShouldBeSameAs(tenant);
        factoryCalls.ShouldBe(2);
    }

    [Fact]
    public async Task Does_pass_the_id_and_cancellation_token_to_the_factory()
    {
        var cache = new NoopTenantCache();
        var token = TestContext.Current.CancellationToken;
        string? receivedId = null;
        var receivedToken = CancellationToken.None;

        await cache.GetOrCreateAsync(
            "tenant1",
            (id, ct) =>
            {
                receivedId = id;
                receivedToken = ct;
                return ValueTask.FromResult<Tenant?>(null);
            },
            token);

        receivedId.ShouldBe("tenant1");
        receivedToken.ShouldBe(token);
    }

    [Fact]
    public async Task Does_complete_evictions_without_doing_anything()
    {
        var cache = new NoopTenantCache();

        await Should.NotThrowAsync(async () => await cache.EvictAsync("tenant1", TestContext.Current.CancellationToken));
        await Should.NotThrowAsync(async () => await cache.EvictAllAsync(TestContext.Current.CancellationToken));
    }
}
