// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.MultiTenancy.Tests.Management;

using Ayaka.MultiTenancy.Management;

public sealed class DefaultTenantManagerTest
{
    [Fact]
    public async Task Does_add_tenant_to_underlying_store()
    {
        var store = A.Fake<ITenantStore>();
        var tenantManager = new DefaultTenantManager(store);
        var tenant = new Tenant("tenant1");

        await tenantManager.AddAsync(tenant, TestContext.Current.CancellationToken);

        A.CallTo(() => store.AddAsync(tenant, A<CancellationToken>.Ignored))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Does_update_tenant_in_underlying_store()
    {
        var store = A.Fake<ITenantStore>();
        var tenantManager = new DefaultTenantManager(store);
        var tenant = new Tenant("tenant1");

        await tenantManager.UpdateAsync(tenant, TestContext.Current.CancellationToken);

        A.CallTo(() => store.UpdateAsync(tenant, A<CancellationToken>.Ignored))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Does_remove_tenant_from_underlying_store()
    {
        var store = A.Fake<ITenantStore>();
        var tenantManager = new DefaultTenantManager(store);
        var tenantId = "tenant1";

        await tenantManager.RemoveAsync(tenantId, TestContext.Current.CancellationToken);

        A.CallTo(() => store.RemoveAsync(tenantId, A<CancellationToken>.Ignored))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Does_report_the_removal_outcome_of_the_underlying_store()
    {
        var store = A.Fake<ITenantStore>();
        var tenantManager = new DefaultTenantManager(store);

        A.CallTo(() => store.RemoveAsync("tenant1", A<CancellationToken>.Ignored))
            .Returns(false);

        var result = await tenantManager.RemoveAsync("tenant1", TestContext.Current.CancellationToken);

        result.ShouldBeFalse();
    }

    [Fact]
    public async Task Does_return_a_single_tenant()
    {
        var store = A.Fake<ITenantStore>();
        var tenantManager = new DefaultTenantManager(store);
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
        var tenantManager = new DefaultTenantManager(store);
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
        var tenantManager = new DefaultTenantManager(store);
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
}
