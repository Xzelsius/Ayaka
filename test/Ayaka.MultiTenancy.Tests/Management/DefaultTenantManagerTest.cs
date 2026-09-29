// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.MultiTenancy.Tests.Management;

using Ayaka.MultiTenancy.Management;
using Microsoft.Extensions.Logging.Abstractions;

public sealed class DefaultTenantManagerTest
{
    [Fact]
    public async Task Does_add_tenant_to_underlying_store()
    {
        var store = A.Fake<ITenantStore>();
        var tenantManager = new DefaultTenantManager(store, null, NullLoggerFactory.Instance);
        var tenant = new Tenant("tenant1");

        await tenantManager.AddAsync(tenant, TestContext.Current.CancellationToken);

        A.CallTo(() => store.AddAsync(tenant, A<CancellationToken>.Ignored))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Does_update_tenant_in_underlying_store()
    {
        var store = A.Fake<ITenantStore>();
        var tenantManager = new DefaultTenantManager(store, null, NullLoggerFactory.Instance);
        var tenant = new Tenant("tenant1");

        await tenantManager.UpdateAsync(tenant, TestContext.Current.CancellationToken);

        A.CallTo(() => store.UpdateAsync(tenant, A<CancellationToken>.Ignored))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Does_remove_tenant_from_underlying_store()
    {
        var store = A.Fake<ITenantStore>();
        var tenantManager = new DefaultTenantManager(store, null, NullLoggerFactory.Instance);
        var tenantId = "tenant1";

        await tenantManager.RemoveAsync(tenantId, TestContext.Current.CancellationToken);

        A.CallTo(() => store.RemoveAsync(tenantId, A<CancellationToken>.Ignored))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Does_report_the_removal_outcome_of_the_underlying_store()
    {
        var store = A.Fake<ITenantStore>();
        var tenantManager = new DefaultTenantManager(store, null, NullLoggerFactory.Instance);

        A.CallTo(() => store.RemoveAsync("tenant1", A<CancellationToken>.Ignored))
            .Returns(false);

        var result = await tenantManager.RemoveAsync("tenant1", TestContext.Current.CancellationToken);

        result.ShouldBeFalse();
    }

    [Fact]
    public async Task Does_return_a_single_tenant()
    {
        var store = A.Fake<ITenantStore>();
        var tenantManager = new DefaultTenantManager(store, null, NullLoggerFactory.Instance);
        var tenantId = "tenant1";
        var tenant = new Tenant(tenantId);

        A.CallTo(() => store.GetAsync(tenantId, A<CancellationToken>.Ignored))
            .Returns(tenant);

        var result = await tenantManager.GetAsync(tenantId, TestContext.Current.CancellationToken);

        result.ShouldBe(tenant);
    }

    [Fact]
    public async Task Does_return_null_if_tenant_not_found()
    {
        var store = A.Fake<ITenantStore>();
        var tenantManager = new DefaultTenantManager(store, null, NullLoggerFactory.Instance);
        var tenantId = "tenant1";

        A.CallTo(() => store.GetAsync(tenantId, A<CancellationToken>.Ignored))
            .Returns((Tenant?)null);

        var result = await tenantManager.GetAsync(tenantId, TestContext.Current.CancellationToken);

        result.ShouldBeNull();
    }

    [Fact]
    public async Task Does_return_all_tenants()
    {
        var store = A.Fake<ITenantStore>();
        var tenantManager = new DefaultTenantManager(store, null, NullLoggerFactory.Instance);
        var tenants = new List<Tenant>
        {
            new("tenant1"),
            new("tenant2")
        };

        A.CallTo(() => store.GetAllAsync(A<CancellationToken>.Ignored))
            .Returns(tenants);

        var result = await tenantManager.GetAllAsync(TestContext.Current.CancellationToken);

        result.ShouldBeEquivalentTo(tenants);
    }

    [Fact]
    public async Task Does_use_cache_to_return_a_single_tenant()
    {
        var store = A.Fake<ITenantStore>();
        var cache = A.Fake<ITenantCache>();
        var tenant = new Tenant("tenant1");
        var tenantManager = new DefaultTenantManager(store, cache, NullLoggerFactory.Instance);

        A.CallTo(() => cache.GetOrCreateAsync(
                tenant.Id,
                A<Func<string, CancellationToken, ValueTask<Tenant?>>>.Ignored,
                A<CancellationToken>.Ignored))
            .Returns(ValueTask.FromResult<Tenant?>(tenant));

        var result = await tenantManager.GetAsync(tenant.Id, TestContext.Current.CancellationToken);

        result.ShouldBeSameAs(tenant);
        A.CallTo(() => store.GetAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Does_evict_cached_tenant_after_adding_to_store()
    {
        var store = A.Fake<ITenantStore>();
        var cache = A.Fake<ITenantCache>();
        var tenant = new Tenant("tenant1");
        var calls = new List<string>();
        var tenantManager = new DefaultTenantManager(store, cache, NullLoggerFactory.Instance);

        A.CallTo(() => store.AddAsync(tenant, A<CancellationToken>.Ignored))
            .Invokes(() => calls.Add("store"));
        A.CallTo(() => cache.EvictAsync(tenant.Id, A<CancellationToken>.Ignored))
            .Invokes(() => calls.Add("cache"));

        await tenantManager.AddAsync(tenant, TestContext.Current.CancellationToken);

        calls.ShouldBe(["store", "cache"]);
    }

    [Fact]
    public async Task Does_evict_cached_tenant_after_updating_store()
    {
        var store = A.Fake<ITenantStore>();
        var cache = A.Fake<ITenantCache>();
        var tenant = new Tenant("tenant1");
        var tenantManager = new DefaultTenantManager(store, cache, NullLoggerFactory.Instance);

        await tenantManager.UpdateAsync(tenant, TestContext.Current.CancellationToken);

        A.CallTo(() => cache.EvictAsync(tenant.Id, A<CancellationToken>.Ignored))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Does_evict_cached_tenant_when_store_reports_it_was_not_removed()
    {
        var store = A.Fake<ITenantStore>();
        var cache = A.Fake<ITenantCache>();
        var tenantManager = new DefaultTenantManager(store, cache, NullLoggerFactory.Instance);

        A.CallTo(() => store.RemoveAsync("tenant1", A<CancellationToken>.Ignored))
            .Returns(false);

        var removed = await tenantManager.RemoveAsync("tenant1", TestContext.Current.CancellationToken);

        removed.ShouldBeFalse();
        A.CallTo(() => cache.EvictAsync("tenant1", A<CancellationToken>.Ignored))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Does_not_evict_cached_tenant_when_adding_to_store_fails()
    {
        var store = A.Fake<ITenantStore>();
        var cache = A.Fake<ITenantCache>();
        var tenant = new Tenant("tenant1");
        var tenantManager = new DefaultTenantManager(store, cache, NullLoggerFactory.Instance);

        A.CallTo(() => store.AddAsync(tenant, A<CancellationToken>.Ignored))
            .ThrowsAsync(new TenantManagementException());

        var act = async () => await tenantManager.AddAsync(tenant, TestContext.Current.CancellationToken);

        _ = await Should.ThrowAsync<TenantManagementException>(act);
        A.CallTo(() => cache.EvictAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Does_not_fail_a_write_when_cache_eviction_fails()
    {
        var store = A.Fake<ITenantStore>();
        var cache = A.Fake<ITenantCache>();
        var tenant = new Tenant("tenant1");
        var tenantManager = new DefaultTenantManager(store, cache, NullLoggerFactory.Instance);

        A.CallTo(() => cache.EvictAsync(tenant.Id, A<CancellationToken>.Ignored))
            .ThrowsAsync(new InvalidOperationException());

        var act = async () => await tenantManager.AddAsync(tenant, TestContext.Current.CancellationToken);

        await Should.NotThrowAsync(act);
    }

    [Fact]
    public async Task Does_evict_cached_tenant_when_the_caller_cancels_after_the_store_was_updated()
    {
        var store = A.Fake<ITenantStore>();
        var cache = A.Fake<ITenantCache>();
        var tenant = new Tenant("tenant1");
        var tenantManager = new DefaultTenantManager(store, cache, NullLoggerFactory.Instance);
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);

        A.CallTo(() => store.UpdateAsync(tenant, A<CancellationToken>.Ignored))
            .Invokes(() => cancellation.Cancel());
        A.CallTo(() => cache.EvictAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .ReturnsLazily((string _, CancellationToken token) =>
                token.IsCancellationRequested ? ValueTask.FromCanceled(token) : ValueTask.CompletedTask);

        // Awaited directly: Should.NotThrowAsync treats a cancelled task as success
        await tenantManager.UpdateAsync(tenant, cancellation.Token);

        A.CallTo(() => cache.EvictAsync(tenant.Id, A<CancellationToken>.Ignored))
            .MustHaveHappenedOnceExactly();
    }
}
