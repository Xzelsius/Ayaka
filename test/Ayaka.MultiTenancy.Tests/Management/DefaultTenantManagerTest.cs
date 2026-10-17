// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.MultiTenancy.Tests.Management;

using Ayaka.MultiTenancy.Management;
using Microsoft.Extensions.Logging.Abstractions;

public sealed class DefaultTenantManagerTest
{
    [Fact]
    public async Task Rejects_adding_a_null_tenant()
    {
        var store = A.Fake<ITenantStore>();
        var tenantManager = new DefaultTenantManager(store, new NoopTenantCache(), NullLoggerFactory.Instance);

        await Should.ThrowAsync<ArgumentNullException>(
            async () => await tenantManager.AddAsync(null!, TestContext.Current.CancellationToken));

        A.CallTo(store).MustNotHaveHappened();
    }

    [Fact]
    public async Task Does_add_tenant_to_underlying_store()
    {
        var store = A.Fake<ITenantStore>();
        var tenantManager = new DefaultTenantManager(store, new NoopTenantCache(), NullLoggerFactory.Instance);
        var tenant = new Tenant("tenant1");

        await tenantManager.AddAsync(tenant, TestContext.Current.CancellationToken);

        A.CallTo(() => store.AddAsync(tenant, A<CancellationToken>.Ignored))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Rejects_updating_a_null_tenant()
    {
        var store = A.Fake<ITenantStore>();
        var tenantManager = new DefaultTenantManager(store, new NoopTenantCache(), NullLoggerFactory.Instance);

        await Should.ThrowAsync<ArgumentNullException>(
            async () => await tenantManager.UpdateAsync(null!, TestContext.Current.CancellationToken));

        A.CallTo(store).MustNotHaveHappened();
    }

    [Fact]
    public async Task Does_update_tenant_in_underlying_store()
    {
        var store = A.Fake<ITenantStore>();
        var tenantManager = new DefaultTenantManager(store, new NoopTenantCache(), NullLoggerFactory.Instance);
        var tenant = new Tenant("tenant1");

        await tenantManager.UpdateAsync(tenant, TestContext.Current.CancellationToken);

        A.CallTo(() => store.UpdateAsync(tenant, A<CancellationToken>.Ignored))
            .MustHaveHappened();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task Rejects_removing_a_tenant_with_an_invalid_id(string? id)
    {
        var store = A.Fake<ITenantStore>();
        var tenantManager = new DefaultTenantManager(store, new NoopTenantCache(), NullLoggerFactory.Instance);

        await Should.ThrowAsync<ArgumentException>(
            async () => await tenantManager.RemoveAsync(id!, TestContext.Current.CancellationToken));

        A.CallTo(store).MustNotHaveHappened();
    }

    [Fact]
    public async Task Does_remove_tenant_from_underlying_store()
    {
        var store = A.Fake<ITenantStore>();
        var tenantManager = new DefaultTenantManager(store, new NoopTenantCache(), NullLoggerFactory.Instance);
        var tenantId = "tenant1";

        await tenantManager.RemoveAsync(tenantId, TestContext.Current.CancellationToken);

        A.CallTo(() => store.RemoveAsync(tenantId, A<CancellationToken>.Ignored))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Does_report_the_removal_outcome_of_the_underlying_store()
    {
        var store = A.Fake<ITenantStore>();
        var tenantManager = new DefaultTenantManager(store, new NoopTenantCache(), NullLoggerFactory.Instance);

        A.CallTo(() => store.RemoveAsync("tenant1", A<CancellationToken>.Ignored))
            .Returns(false);

        var result = await tenantManager.RemoveAsync("tenant1", TestContext.Current.CancellationToken);

        result.ShouldBeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task Rejects_getting_a_tenant_with_an_invalid_id(string? id)
    {
        var store = A.Fake<ITenantStore>();
        var tenantManager = new DefaultTenantManager(store, new NoopTenantCache(), NullLoggerFactory.Instance);

        await Should.ThrowAsync<ArgumentException>(
            async () => await tenantManager.GetAsync(id!, TestContext.Current.CancellationToken));

        A.CallTo(store).MustNotHaveHappened();
    }

    [Fact]
    public async Task Does_return_a_single_tenant()
    {
        var store = A.Fake<ITenantStore>();
        var tenantManager = new DefaultTenantManager(store, new NoopTenantCache(), NullLoggerFactory.Instance);
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
        var tenantManager = new DefaultTenantManager(store, new NoopTenantCache(), NullLoggerFactory.Instance);
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
        var tenantManager = new DefaultTenantManager(store, new NoopTenantCache(), NullLoggerFactory.Instance);
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
        var tenantManager = new DefaultTenantManager(store, cache, NullLoggerFactory.Instance);

        await tenantManager.AddAsync(tenant, TestContext.Current.CancellationToken);

        A.CallTo(() => store.AddAsync(tenant, A<CancellationToken>.Ignored))
            .MustHaveHappened()
            .Then(A.CallTo(() => cache.EvictAsync(tenant.Id, A<CancellationToken>.Ignored)).MustHaveHappened());
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

        await Should.ThrowAsync<TenantManagementException>(
            async () => await tenantManager.AddAsync(tenant, TestContext.Current.CancellationToken));

        A.CallTo(() => cache.EvictAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .MustNotHaveHappened();
    }

    [Fact]
    public Task Does_not_fail_adding_when_cache_eviction_fails()
    {
        var store = A.Fake<ITenantStore>();
        var cache = A.Fake<ITenantCache>();
        var tenant = new Tenant("tenant1");
        var tenantManager = new DefaultTenantManager(store, cache, NullLoggerFactory.Instance);

        A.CallTo(() => cache.EvictAsync(tenant.Id, A<CancellationToken>.Ignored))
            .ThrowsAsync(new InvalidOperationException());

        return Should.NotThrowAsync(async () => await tenantManager.AddAsync(tenant, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Does_evict_cached_tenant_after_updating_store()
    {
        var store = A.Fake<ITenantStore>();
        var cache = A.Fake<ITenantCache>();
        var tenant = new Tenant("tenant1");
        var tenantManager = new DefaultTenantManager(store, cache, NullLoggerFactory.Instance);

        await tenantManager.UpdateAsync(tenant, TestContext.Current.CancellationToken);

        A.CallTo(() => store.UpdateAsync(tenant, A<CancellationToken>.Ignored))
            .MustHaveHappened()
            .Then(A.CallTo(() => cache.EvictAsync(tenant.Id, A<CancellationToken>.Ignored)).MustHaveHappened());
    }

    [Fact]
    public Task Does_not_fail_an_update_when_cache_eviction_fails()
    {
        var store = A.Fake<ITenantStore>();
        var cache = A.Fake<ITenantCache>();
        var tenant = new Tenant("tenant1");
        var tenantManager = new DefaultTenantManager(store, cache, NullLoggerFactory.Instance);

        A.CallTo(() => cache.EvictAsync(tenant.Id, A<CancellationToken>.Ignored))
            .ThrowsAsync(new InvalidOperationException());

        return Should.NotThrowAsync(async () => await tenantManager.UpdateAsync(tenant, TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Does_evict_cached_tenant_regardless_of_store_removal_outcome(bool removed)
    {
        var store = A.Fake<ITenantStore>();
        var cache = A.Fake<ITenantCache>();
        var tenantManager = new DefaultTenantManager(store, cache, NullLoggerFactory.Instance);

        A.CallTo(() => store.RemoveAsync("tenant1", A<CancellationToken>.Ignored))
            .Returns(removed);

        var outcome = await tenantManager.RemoveAsync("tenant1", TestContext.Current.CancellationToken);
        outcome.ShouldBe(removed);

        A.CallTo(() => store.RemoveAsync("tenant1", A<CancellationToken>.Ignored))
            .MustHaveHappened()
            .Then(A.CallTo(() => cache.EvictAsync("tenant1", A<CancellationToken>.Ignored)).MustHaveHappened());
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
            .Invokes(cancellation.Cancel);
        A.CallTo(() => cache.EvictAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .ReturnsLazily((string _, CancellationToken token) =>
                token.IsCancellationRequested ? ValueTask.FromCanceled(token) : ValueTask.CompletedTask);

        // Awaited directly: Should.NotThrowAsync treats a cancelled task as success
        await tenantManager.UpdateAsync(tenant, cancellation.Token);

        A.CallTo(() => cache.EvictAsync(tenant.Id, CancellationToken.None))
            .MustHaveHappenedOnceExactly();
    }
}
