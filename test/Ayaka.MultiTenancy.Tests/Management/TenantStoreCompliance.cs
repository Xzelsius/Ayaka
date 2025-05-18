// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.MultiTenancy.Tests.Management;

using System.Collections.Immutable;
using Ayaka.MultiTenancy.Management;

public abstract class TenantStoreCompliance<TStoreFixture> : IDisposable, IAsyncDisposable, IAsyncLifetime
    where TStoreFixture : TenantStoreFixture, new()
{
    protected TenantStoreFixture StoreFixture { get; } = new TStoreFixture();

    [Fact]
    public async Task Allows_adding_a_new_tenant()
    {
        var store = StoreFixture.Store;
        var tenant = new Tenant("tenant1");

        await store.AddAsync(tenant);

        var tenants = await store.GetAllAsync();
        tenants.Should().ContainSingle();
        tenants.Should().ContainEquivalentOf(tenant);
    }

    [Fact]
    public async Task Allows_adding_a_new_tenant_with_a_display_name()
    {
        var store = StoreFixture.Store;
        var tenant = new Tenant("tenant1", "Tenant 1");

        await store.AddAsync(tenant);

        var tenants = await store.GetAllAsync();
        tenants.Should().ContainSingle();
        tenants.Should().ContainEquivalentOf(tenant);
    }

    [Fact]
    public async Task Allows_adding_a_new_tenant_with_attributes()
    {
        var store = StoreFixture.Store;
        var tenant = new Tenant(
            "tenant1",
            "Tenant 1",
            new Dictionary<string, string>
            {
                { "key", "value" }
            }.ToImmutableDictionary());

        await store.AddAsync(tenant);

        var tenants = await store.GetAllAsync();
        tenants.Should().ContainSingle();
        tenants.Should().ContainEquivalentOf(tenant);
    }

    [Fact]
    public async Task Allows_adding_many_tenants_parallel()
    {
        var store = StoreFixture.Store;

        await Parallel.ForEachAsync(
            Enumerable.Range(1, 100),
            new ParallelOptions { MaxDegreeOfParallelism = 10 },
            async (i, ct) => await store.AddAsync(new Tenant("tenant" + i), ct));

        var tenants = await store.GetAllAsync();
        tenants.Should().HaveCount(100);
    }

    [Fact]
    public async Task Throws_if_tenant_already_exists()
    {
        var store = StoreFixture.Store;
        await store.AddAsync(new Tenant("tenant1"));

        var act = async () => await store.AddAsync(new Tenant("tenant1"));

        await act.Should().ThrowAsync<TenantManagementException>().WithMessage("Tenant with id 'tenant1' already exists");
    }

    [Fact]
    public async Task Allows_updating_the_display_name_of_an_existing_tenant()
    {
        var store = StoreFixture.Store;
        var tenant = new Tenant("tenant1", "Tenant 1");

        await store.AddAsync(tenant);

        var updatedTenant = tenant with
        {
            DisplayName = "Tenant 1 (Updated)",
        };
        await store.UpdateAsync(updatedTenant);

        var tenants = await store.GetAllAsync();
        tenants.Should().ContainSingle();
        tenants.Should().ContainEquivalentOf(updatedTenant);
    }

    [Fact]
    public async Task Allows_updating_the_attributes_of_an_existing_tenant()
    {
        var store = StoreFixture.Store;
        var tenant = new Tenant(
            "tenant1",
            "Tenant 1",
            new Dictionary<string, string>
            {
                { "key1", "value" }
            }.ToImmutableDictionary());

        await store.AddAsync(tenant);

        var updatedTenant = tenant with
        {
            Attributes = tenant.Attributes?.Add("key2", "value2"),
        };
        await store.UpdateAsync(updatedTenant);

        var tenants = await store.GetAllAsync();
        tenants.Should().ContainSingle();
        tenants.Should().ContainEquivalentOf(updatedTenant);
    }

    [Fact]
    public Task Throws_when_updating_non_existing_tenant()
    {
        var store = StoreFixture.Store;
        var tenant = new Tenant(
            "tenant1",
            "Tenant 1",
            new Dictionary<string, string>
            {
                { "key1", "value" }
            }.ToImmutableDictionary());

        var updatedTenant = tenant with
        {
            Attributes = tenant.Attributes?.Add("key2", "value2"),
        };

        var act = async () => await store.UpdateAsync(updatedTenant);

        return act.Should().ThrowAsync<TenantManagementException>().WithMessage("Tenant with id 'tenant1' does not exist");
    }

    [Fact]
    public async Task Allows_removing_existing_tenant()
    {
        var store = StoreFixture.Store;
        await store.AddAsync(new Tenant("tenant1"));

        // Make sure the tenant was added
        var tenants = await store.GetAllAsync();
        tenants.Should().ContainSingle();

        await store.RemoveAsync("tenant1");

        tenants = await store.GetAllAsync();
        tenants.Should().BeEmpty();
    }

    [Fact]
    public Task Does_not_throw_when_removing_non_existing_tenant()
    {
        var store = StoreFixture.Store;

        var act = async () => await store.RemoveAsync("tenant1");

        return act.Should().NotThrowAsync();
    }

    public Task InitializeAsync()
    {
        if (StoreFixture is IAsyncLifetime asyncLifetime)
        {
            return asyncLifetime.InitializeAsync();
        }

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    async Task IAsyncLifetime.DisposeAsync()
        => await DisposeAsync();

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore();
        Dispose(false);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            StoreFixture.Dispose();
        }
    }

    protected virtual ValueTask DisposeAsyncCore()
        => StoreFixture.DisposeAsync();
}
