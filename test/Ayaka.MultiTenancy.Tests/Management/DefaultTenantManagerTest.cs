// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.MultiTenancy.Tests.Management;

using Ayaka.MultiTenancy.Management;

public sealed class DefaultTenantManagerTest
{
    [Fact]
    public async Task Does_add_tenant_to_underlying_storage()
    {
        var storage = A.Fake<ITenantStorage>();
        var tenantManager = new DefaultTenantManager(storage);
        var tenant = new Tenant("tenant1");

        await tenantManager.AddAsync(tenant);

        A.CallTo(() => storage.AddAsync(tenant, A<CancellationToken>.Ignored))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Does_remove_tenant_from_underlying_storage()
    {
        var storage = A.Fake<ITenantStorage>();
        var tenantManager = new DefaultTenantManager(storage);
        var tenantId = "tenant1";

        await tenantManager.RemoveAsync(tenantId);

        A.CallTo(() => storage.RemoveAsync(tenantId, A<CancellationToken>.Ignored))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Does_return_tenant_if_found()
    {
        var storage = A.Fake<ITenantStorage>();
        var tenantManager = new DefaultTenantManager(storage);
        var tenantId = "tenant1";
        var tenant = new Tenant(tenantId);

        A.CallTo(() => storage.GetAllAsync(A<CancellationToken>.Ignored))
            .Returns([tenant]);

        var result = await tenantManager.GetAsync(tenantId);

        result.Should().Be(tenant);
    }

    [Fact]
    public async Task Does_return_null_if_tenant_not_found()
    {
        var storage = A.Fake<ITenantStorage>();
        var tenantManager = new DefaultTenantManager(storage);
        var tenantId = "tenant1";

        A.CallTo(() => storage.GetAllAsync(A<CancellationToken>.Ignored))
            .Returns([]);

        var result = await tenantManager.GetAsync(tenantId);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Does_return_all_tenants()
    {
        var storage = A.Fake<ITenantStorage>();
        var tenantManager = new DefaultTenantManager(storage);
        var tenants = new List<Tenant>
        {
            new("tenant1"),
            new("tenant2")
        };

        A.CallTo(() => storage.GetAllAsync(A<CancellationToken>.Ignored))
            .Returns(tenants);

        var result = await tenantManager.GetAllAsync();

        result.Should().BeEquivalentTo(tenants);
    }
}
