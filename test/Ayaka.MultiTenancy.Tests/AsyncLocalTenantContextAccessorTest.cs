// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.MultiTenancy.Tests;

public sealed class AsyncLocalTenantContextAccessorTest
{
    [Fact]
    public async Task Getting_TenantContext_returns_TenantContext()
    {
        var context = new TenantContext("foo", "bar");
        var accessor = new AsyncLocalTenantContextAccessor
        {
            TenantContext = context
        };

        await Task.Delay(100);

        accessor.TenantContext.Should().BeSameAs(context);
    }

    [Fact]
    public void Getting_TenantContext_returns_null_when_no_TenantContext_is_set()
    {
        var accessor = new AsyncLocalTenantContextAccessor();

        accessor.TenantContext.Should().BeNull();
    }

    [Fact]
    [SuppressMessage("AsyncUsage", "AsyncFixer03:Fire-and-forget async-void methods or delegates", Justification = "AsyncLocal<T> testing")]
    public async Task Getting_TenantContext_returns_null_when_set_to_null_within_current_context_flow()
    {
        var context = new TenantContext("foo", "bar");
        var accessor = new AsyncLocalTenantContextAccessor
        {
            TenantContext = context
        };

        var checkAsyncFlowTcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var waitForNullTcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var afterNullCheckTcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        ThreadPool.QueueUserWorkItem(async _ =>
        {
            // Make sure the TenantContext flows with the execution context
            accessor.TenantContext.Should().BeSameAs(context);

            // Signal the outer code to continue
            checkAsyncFlowTcs.SetResult();

            // Wait for continuation signal from the outer code
            await waitForNullTcs.Task;

            try
            {
                // Make sure the TenantContext is now null
                accessor.TenantContext.Should().BeNull();

                // Signal the outer code to continue
                afterNullCheckTcs.SetResult();
            }
            catch (Exception e)
            {
                // Report the exception to the outer code
                afterNullCheckTcs.SetException(e);
            }
        });

        // Wait for the async flow check to complete
        // (first assertion in the thread pool callback)
        await checkAsyncFlowTcs.Task;

        // Set the TenantContext to null
        accessor.TenantContext = null;

        // Signal the thread pool callback to continue
        waitForNullTcs.SetResult();

        accessor.TenantContext.Should().BeNull();

        // Wait for the thread pool callback to complete
        await afterNullCheckTcs.Task;
    }

    [Fact]
    [SuppressMessage("AsyncUsage", "AsyncFixer03:Fire-and-forget async-void methods or delegates", Justification = "AsyncLocal<T> testing")]
    public async Task Getting_TenantContext_returns_null_when_changed_within_current_context_flow()
    {
        var context = new TenantContext("foo", "bar");
        var accessor = new AsyncLocalTenantContextAccessor
        {
            TenantContext = context
        };

        var checkAsyncFlowTcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var waitForNullTcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var afterNullCheckTcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        ThreadPool.QueueUserWorkItem(async _ =>
        {
            // Make sure the TenantContext flows with the execution context
            accessor.TenantContext.Should().BeSameAs(context);

            // Signal the outer code to continue
            checkAsyncFlowTcs.SetResult();

            // Wait for continuation signal from the outer code
            await waitForNullTcs.Task;

            try
            {
                // Make sure the TenantContext is now null
                accessor.TenantContext.Should().BeNull();

                // Signal the outer code to continue
                afterNullCheckTcs.SetResult();
            }
            catch (Exception e)
            {
                // Report the exception to the outer code
                afterNullCheckTcs.SetException(e);
            }
        });

        // Wait for the async flow check to complete
        // (first assertion in the thread pool callback)
        await checkAsyncFlowTcs.Task;

        // Set the TenantContext to something new
        var context2 = new TenantContext("baz", "qux");
        accessor.TenantContext = context2;

        // Signal the thread pool callback to continue
        waitForNullTcs.SetResult();

        accessor.TenantContext.Should().BeSameAs(context2);

        // Wait for the thread pool callback to complete
        await afterNullCheckTcs.Task;
    }

    [Fact]
    [SuppressMessage("AsyncUsage", "AsyncFixer01:The method does not need to use async/ await.", Justification = "AsyncLocal<T> testing")]
    public async Task Getting_TenantContext_does_not_flow_if_accessor_set_to_null()
    {
        var context = new TenantContext("foo", "bar");
        var accessor = new AsyncLocalTenantContextAccessor
        {
            TenantContext = context
        };

        var checkAsyncFlowTcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        accessor.TenantContext = null;

        ThreadPool.QueueUserWorkItem(_ =>
        {
            try
            {
                // Make sure the TenantContext flows with the execution context
                accessor.TenantContext.Should().BeNull();

                // Signal the outer code to continue
                checkAsyncFlowTcs.SetResult();
            }
            catch (Exception e)
            {
                // Report the exception to the outer code
                checkAsyncFlowTcs.SetException(e);
            }
        });

        await checkAsyncFlowTcs.Task;
    }

    [Fact]
    [SuppressMessage("AsyncUsage", "AsyncFixer01:The method does not need to use async/ await.", Justification = "AsyncLocal<T> testing")]
    public async Task Getting_TenantContext_does_not_flow_if_ExecutionContext_does_not_flow()
    {
        var context = new TenantContext("foo", "bar");
        var accessor = new AsyncLocalTenantContextAccessor
        {
            TenantContext = context
        };

        var checkAsyncFlowTcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        ThreadPool.UnsafeQueueUserWorkItem(_ =>
        {
            try
            {
                // Make sure the TenantContext flows with the execution context
                accessor.TenantContext.Should().BeNull();

                // Signal the outer code to continue
                checkAsyncFlowTcs.SetResult();
            }
            catch (Exception e)
            {
                // Report the exception to the outer code
                checkAsyncFlowTcs.SetException(e);
            }
        }, null);

        await checkAsyncFlowTcs.Task;
    }
}
