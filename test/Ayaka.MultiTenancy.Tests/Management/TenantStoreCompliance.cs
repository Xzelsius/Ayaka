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

        await store.AddAsync(tenant, TestContext.Current.CancellationToken);

        var tenants = await store.GetAllAsync(TestContext.Current.CancellationToken);
        tenants.ShouldHaveSingleItem().ShouldBeEquivalentTo(tenant);
    }

    [Fact]
    public async Task Allows_adding_a_new_tenant_with_a_display_name()
    {
        var store = StoreFixture.Store;
        var tenant = new Tenant("tenant1", "Tenant 1");

        await store.AddAsync(tenant, TestContext.Current.CancellationToken);

        var tenants = await store.GetAllAsync(TestContext.Current.CancellationToken);
        tenants.ShouldHaveSingleItem().ShouldBeEquivalentTo(tenant);
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

        await store.AddAsync(tenant, TestContext.Current.CancellationToken);

        var tenants = await store.GetAllAsync(TestContext.Current.CancellationToken);
        tenants.ShouldHaveSingleItem().ShouldBeEquivalentTo(tenant);
    }

    [Fact]
    public async Task Allows_adding_many_tenants_parallel()
    {
        // Arm every addition first, then release them together so the adds actually overlap.
        var store = StoreFixture.Store;
        var cancellationToken = TestContext.Current.CancellationToken;

        var expectedTenants = Enumerable.Range(1, 100)
            .Select(i => new Tenant("tenant" + i))
            .ToArray();

        var start = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var additions = expectedTenants
            .Select(async tenant =>
            {
                await start.Task.WaitAsync(cancellationToken);
                await store.AddAsync(tenant, cancellationToken);
            })
            .ToArray();

        start.SetResult();
        await Task.WhenAll(additions);

        var storedTenants = await store.GetAllAsync(cancellationToken);
        storedTenants.ShouldBe(expectedTenants, ignoreOrder: true);
    }

    [Fact]
    public async Task Throws_if_tenant_already_exists()
    {
        var store = StoreFixture.Store;
        await store.AddAsync(new Tenant("tenant1"), TestContext.Current.CancellationToken);

        var act = async () => await store.AddAsync(new Tenant("tenant1"));

        (await Should.ThrowAsync<TenantManagementException>(act))
            .Message.ShouldBe("Tenant with id 'tenant1' already exists");
    }

    [Fact]
    public async Task Allows_updating_the_display_name_of_an_existing_tenant()
    {
        var store = StoreFixture.Store;
        var tenant = new Tenant("tenant1", "Tenant 1");

        await store.AddAsync(tenant, TestContext.Current.CancellationToken);

        var updatedTenant = tenant with
        {
            DisplayName = "Tenant 1 (Updated)",
        };
        await store.UpdateAsync(updatedTenant, TestContext.Current.CancellationToken);

        var tenants = await store.GetAllAsync(TestContext.Current.CancellationToken);
        tenants.ShouldHaveSingleItem().ShouldBeEquivalentTo(updatedTenant);
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

        await store.AddAsync(tenant, TestContext.Current.CancellationToken);

        var updatedTenant = tenant with
        {
            Attributes = tenant.Attributes?.Add("key2", "value2"),
        };
        await store.UpdateAsync(updatedTenant, TestContext.Current.CancellationToken);

        var tenants = await store.GetAllAsync(TestContext.Current.CancellationToken);
        tenants.ShouldHaveSingleItem().ShouldBeEquivalentTo(updatedTenant);
    }

    [Fact]
    public async Task Throws_when_updating_non_existing_tenant()
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

        (await Should.ThrowAsync<TenantManagementException>(act))
            .Message.ShouldBe("Tenant with id 'tenant1' does not exist");
    }

    [Fact]
    public async Task Allows_removing_existing_tenant()
    {
        var store = StoreFixture.Store;
        await store.AddAsync(new Tenant("tenant1"), TestContext.Current.CancellationToken);

        // Make sure the tenant was added
        var tenants = await store.GetAllAsync(TestContext.Current.CancellationToken);
        tenants.ShouldHaveSingleItem();

        var removed = await store.RemoveAsync("tenant1", TestContext.Current.CancellationToken);

        removed.ShouldBeTrue();

        tenants = await store.GetAllAsync(TestContext.Current.CancellationToken);
        tenants.ShouldBeEmpty();
    }

    [Fact]
    public async Task Reports_false_when_removing_non_existing_tenant()
    {
        var store = StoreFixture.Store;

        var removed = await store.RemoveAsync("tenant1", TestContext.Current.CancellationToken);

        removed.ShouldBeFalse();
    }

    [Fact]
    public async Task Allows_getting_an_existing_tenant()
    {
        var store = StoreFixture.Store;
        var tenant = new Tenant(
            "tenant1",
            "Tenant 1",
            new Dictionary<string, string>
            {
                { "key", "value" }
            }.ToImmutableDictionary());

        await store.AddAsync(tenant, TestContext.Current.CancellationToken);

        var found = await store.GetAsync("tenant1", TestContext.Current.CancellationToken);

        found.ShouldNotBeNull().ShouldBeEquivalentTo(tenant);
    }

    [Fact]
    public async Task Returns_null_when_getting_a_non_existing_tenant()
    {
        var store = StoreFixture.Store;
        await store.AddAsync(new Tenant("tenant1"), TestContext.Current.CancellationToken);

        var found = await store.GetAsync("tenant2", TestContext.Current.CancellationToken);

        found.ShouldBeNull();
    }

    [Fact]
    public async Task Allows_getting_all_existing_tenants()
    {
        var store = StoreFixture.Store;
        var tenant1 = new Tenant(
            "tenant1",
            "Tenant 1",
            new Dictionary<string, string>
            {
                { "key", "value" }
            }.ToImmutableDictionary());
        var tenant2 = new Tenant(
            "tenant2",
            "Tenant 2",
            new Dictionary<string, string>
            {
                    { "key", "value" }
            }.ToImmutableDictionary());

        await store.AddAsync(tenant1, TestContext.Current.CancellationToken);
        await store.AddAsync(tenant2, TestContext.Current.CancellationToken);

        var found = await store.GetAllAsync(TestContext.Current.CancellationToken);

        found.ShouldBe([tenant1, tenant2], ignoreOrder: true);
    }

    public ValueTask InitializeAsync()
    {
        if (StoreFixture is IAsyncLifetime asyncLifetime)
        {
            return asyncLifetime.InitializeAsync();
        }

        return ValueTask.CompletedTask;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

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
