// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.MultiTenancy.Tests.Management;

using Ayaka.MultiTenancy.Management;

public abstract class TenantStorageFixture : IDisposable, IAsyncDisposable
{
    public abstract ITenantStorage Storage { get; }

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
    }

    protected virtual ValueTask DisposeAsyncCore()
        => ValueTask.CompletedTask;
}
