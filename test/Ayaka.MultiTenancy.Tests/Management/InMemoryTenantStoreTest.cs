// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.MultiTenancy.Tests.Management;

using Ayaka.MultiTenancy.Management;

public sealed class InMemoryStoreFixture : TenantStoreFixture
{
    public override ITenantStore Store { get; } = new InMemoryTenantStore();
}

public sealed class InMemoryTenantStoreTest : TenantStoreCompliance<InMemoryStoreFixture>
{
    [Fact]
    public async Task Does_store_tenants_local_to_the_instance()
    {
        var store1 = new InMemoryTenantStore();
        var store2 = new InMemoryTenantStore();

        await store1.AddAsync(new Tenant("tenant1"));

        var tenantsFromStore1 = await store1.GetAllAsync();
        tenantsFromStore1.Should().ContainSingle();

        var tenantsFromStore2 = await store2.GetAllAsync();
        tenantsFromStore2.Should().BeEmpty();
    }
}
