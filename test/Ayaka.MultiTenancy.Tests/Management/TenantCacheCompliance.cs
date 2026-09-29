// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.MultiTenancy.Tests.Management;

using Ayaka.MultiTenancy.Management;

public abstract class TenantCacheCompliance<TCacheFixture> : IDisposable, IAsyncDisposable
    where TCacheFixture : TenantCacheFixture, new()
{
    protected TenantCacheFixture CacheFixture { get; } = new TCacheFixture();

    [Fact]
    public async Task Caches_a_tenant()
    {
        var tenant = new Tenant("tenant1");
        var factoryCalls = 0;

        async ValueTask<Tenant?> Factory(string _, CancellationToken __)
        {
            factoryCalls++;
            await Task.Yield();
            return tenant;
        }

        var first = await CacheFixture.Cache.GetOrCreateAsync(
            tenant.Id,
            Factory,
            TestContext.Current.CancellationToken);
        var second = await CacheFixture.Cache.GetOrCreateAsync(
            tenant.Id,
            Factory,
            TestContext.Current.CancellationToken);

        first.ShouldBe(tenant);
        second.ShouldBe(tenant);
        factoryCalls.ShouldBe(1);
    }

    [Fact]
    public async Task Caches_a_missing_tenant()
    {
        var factoryCalls = 0;

        ValueTask<Tenant?> Factory(string _, CancellationToken __)
        {
            factoryCalls++;
            return ValueTask.FromResult<Tenant?>(null);
        }

        var first = await CacheFixture.Cache.GetOrCreateAsync(
            "missing",
            Factory,
            TestContext.Current.CancellationToken);
        var second = await CacheFixture.Cache.GetOrCreateAsync(
            "missing",
            Factory,
            TestContext.Current.CancellationToken);

        first.ShouldBeNull();
        second.ShouldBeNull();
        factoryCalls.ShouldBe(1);
    }

    [Fact]
    public async Task Evicting_a_tenant_forces_a_reload()
    {
        var factoryCalls = 0;

        ValueTask<Tenant?> Factory(string id, CancellationToken _)
        {
            factoryCalls++;
            return ValueTask.FromResult<Tenant?>(new Tenant(id));
        }

        _ = await CacheFixture.Cache.GetOrCreateAsync(
            "tenant1",
            Factory,
            TestContext.Current.CancellationToken);

        await CacheFixture.Cache.EvictAsync("tenant1", TestContext.Current.CancellationToken);

        _ = await CacheFixture.Cache.GetOrCreateAsync(
            "tenant1",
            Factory,
            TestContext.Current.CancellationToken);

        factoryCalls.ShouldBe(2);
    }

    [Fact]
    public async Task Evicting_all_tenants_forces_a_reload()
    {
        var factoryCalls = 0;

        ValueTask<Tenant?> Factory(string id, CancellationToken _)
        {
            factoryCalls++;
            return ValueTask.FromResult<Tenant?>(new Tenant(id));
        }

        _ = await CacheFixture.Cache.GetOrCreateAsync(
            "tenant1",
            Factory,
            TestContext.Current.CancellationToken);
        _ = await CacheFixture.Cache.GetOrCreateAsync(
            "tenant2",
            Factory,
            TestContext.Current.CancellationToken);

        await CacheFixture.Cache.EvictAllAsync(TestContext.Current.CancellationToken);

        _ = await CacheFixture.Cache.GetOrCreateAsync(
            "tenant1",
            Factory,
            TestContext.Current.CancellationToken);
        _ = await CacheFixture.Cache.GetOrCreateAsync(
            "tenant2",
            Factory,
            TestContext.Current.CancellationToken);

        factoryCalls.ShouldBe(4);
    }

    [Fact]
    public async Task Concurrent_requests_invoke_the_factory_once()
    {
        // Hold the factory open so every request has to join the in-flight one instead of hitting a completed entry.
        var factoryCalls = 0;
        var tenant = new Tenant("tenant1");
        var factoryStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseFactory = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        async ValueTask<Tenant?> Factory(string _, CancellationToken ct)
        {
            Interlocked.Increment(ref factoryCalls);
            factoryStarted.TrySetResult();
            await releaseFactory.Task.WaitAsync(ct);
            return tenant;
        }

        var requests = Enumerable.Range(0, 20)
            .Select(_ => CacheFixture.Cache.GetOrCreateAsync(
                "tenant1",
                Factory,
                TestContext.Current.CancellationToken).AsTask())
            .ToArray();

        await factoryStarted.Task.WaitAsync(TestContext.Current.CancellationToken);
        releaseFactory.SetResult();
        var tenants = await Task.WhenAll(requests);

        factoryCalls.ShouldBe(1);
        tenants.ShouldAllBe(result => result == tenant);
    }

    [Fact]
    public async Task Evicting_a_tenant_discards_a_load_that_is_in_progress()
    {
        var loader = new GatedLoader { Current = new Tenant("tenant1", "before") };

        await LoadThenChangeAsync(
            loader,
            new Tenant("tenant1", "after"),
            () => CacheFixture.Cache.EvictAsync("tenant1", TestContext.Current.CancellationToken));

        var tenant = await CacheFixture.Cache.GetOrCreateAsync(
            "tenant1",
            loader.LoadAsync,
            TestContext.Current.CancellationToken);

        tenant.ShouldNotBeNull().DisplayName.ShouldBe("after");
    }

    [Fact]
    public async Task Evicting_a_tenant_discards_a_miss_that_is_in_progress()
    {
        var loader = new GatedLoader { Current = null };

        await LoadThenChangeAsync(
            loader,
            new Tenant("tenant1"),
            () => CacheFixture.Cache.EvictAsync("tenant1", TestContext.Current.CancellationToken));

        var tenant = await CacheFixture.Cache.GetOrCreateAsync(
            "tenant1",
            loader.LoadAsync,
            TestContext.Current.CancellationToken);

        tenant.ShouldNotBeNull();
    }

    [Fact]
    public async Task Evicting_all_tenants_discards_a_load_that_is_in_progress()
    {
        var loader = new GatedLoader { Current = new Tenant("tenant1", "before") };

        await LoadThenChangeAsync(
            loader,
            new Tenant("tenant1", "after"),
            () => CacheFixture.Cache.EvictAllAsync(TestContext.Current.CancellationToken));

        var tenant = await CacheFixture.Cache.GetOrCreateAsync(
            "tenant1",
            loader.LoadAsync,
            TestContext.Current.CancellationToken);

        tenant.ShouldNotBeNull().DisplayName.ShouldBe("after");
    }

    public void Dispose()
    {
        CacheFixture.Dispose();
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        await CacheFixture.DisposeAsync();
        GC.SuppressFinalize(this);
    }

    // Starts a load that reads the old state, then changes the state and evicts while that load is still running.
    private async Task LoadThenChangeAsync(GatedLoader loader, Tenant? changed, Func<ValueTask> evict)
    {
        var inProgress = CacheFixture.Cache.GetOrCreateAsync(
                "tenant1",
                loader.LoadAsync,
                TestContext.Current.CancellationToken)
            .AsTask();
        await loader.Started.WaitAsync(TestContext.Current.CancellationToken);

        loader.Current = changed;
        await evict();
        loader.Release();

        _ = await inProgress;
    }

    // Loads the current state immediately, but holds back the result of the first load until released.
    private sealed class GatedLoader
    {
        private readonly TaskCompletionSource _started = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource _released = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private int _loads;

        public Tenant? Current { get; set; }

        public Task Started => _started.Task;

        public void Release()
            => _released.TrySetResult();

        public async ValueTask<Tenant?> LoadAsync(string _, CancellationToken cancellationToken)
        {
            var snapshot = Current;
            if (Interlocked.Increment(ref _loads) == 1)
            {
                _started.TrySetResult();
                await _released.Task.WaitAsync(cancellationToken);
            }

            return snapshot;
        }
    }
}
