// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.MultiTenancy.Tests.Management;

using Ayaka.MultiTenancy.Management;

public sealed class InMemoryStorageFixture : TenantStorageFixture
{
    public override ITenantStorage Storage { get; } = new InMemoryTenantStorage();
}

public sealed class InMemoryTenantStorageTest : TenantStorageCompliance<InMemoryStorageFixture>
{
    [Fact]
    public async Task Does_store_tenants_local_to_the_instance()
    {
        var storage1 = new InMemoryTenantStorage();
        var storage2 = new InMemoryTenantStorage();

        await storage1.AddAsync(new Tenant("tenant1"));

        var tenantsFromStorage1 = await storage1.GetAllAsync();
        tenantsFromStorage1.Should().ContainSingle();

        var tenantsFromStorage2 = await storage2.GetAllAsync();
        tenantsFromStorage2.Should().BeEmpty();
    }
}
