// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.MultiTenancy.Tests.Management;

using Ayaka.MultiTenancy.Management;

public abstract class TenantStorageCompliance<TStorageFixture> : IDisposable, IAsyncDisposable, IAsyncLifetime
    where TStorageFixture : TenantStorageFixture, new()
{
    protected TenantStorageFixture StorageFixture { get; } = new TStorageFixture();

    [Fact]
    public async Task Allows_adding_a_new_tenant()
    {
        var storage = StorageFixture.Storage;
        var tenant = new Tenant("tenant1");

        await storage.AddAsync(tenant);

        var tenants = await storage.GetAllAsync();
        tenants.Should().ContainSingle();
        tenants.Should().Contain(tenant);
    }

    [Fact]
    public async Task Allows_adding_many_tenants_parallel()
    {
        var storage = StorageFixture.Storage;

        await Parallel.ForEachAsync(
            Enumerable.Range(1, 100),
            new ParallelOptions { MaxDegreeOfParallelism = 10 },
            async (i, ct) => await storage.AddAsync(new Tenant("tenant" + i), ct));

        var tenants = await storage.GetAllAsync();
        tenants.Should().HaveCount(100);
    }

    [Fact]
    public async Task Throws_if_tenant_already_exists()
    {
        var storage = StorageFixture.Storage;
        await storage.AddAsync(new Tenant("tenant1"));

        var act = async () => await storage.AddAsync(new Tenant("tenant1"));

        await act.Should().ThrowAsync<TenantManagementException>().WithMessage("Tenant with id 'tenant1' already exists");
    }

    [Fact]
    public async Task Allows_removing_existing_tenant()
    {
        var storage = StorageFixture.Storage;
        await storage.AddAsync(new Tenant("tenant1"));

        // Make sure the tenant was added
        var tenants = await storage.GetAllAsync();
        tenants.Should().ContainSingle();

        await storage.RemoveAsync("tenant1");

        tenants = await storage.GetAllAsync();
        tenants.Should().BeEmpty();
    }

    [Fact]
    public Task Does_not_throw_when_removing_non_existing_tenant()
    {
        var storage = StorageFixture.Storage;

        var act = async () => await storage.RemoveAsync("tenant1");

        return act.Should().NotThrowAsync();
    }

    public Task InitializeAsync()
    {
        if (StorageFixture is IAsyncLifetime asyncLifetime)
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
            StorageFixture.Dispose();
        }
    }

    protected virtual ValueTask DisposeAsyncCore()
        => StorageFixture.DisposeAsync();
}
