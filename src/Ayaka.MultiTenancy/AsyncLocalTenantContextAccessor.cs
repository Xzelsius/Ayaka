// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.MultiTenancy;

using System.Diagnostics;

/// <summary>
///     Provides access to the current <see cref="TenantContext"/>, if one is available.
/// </summary>
/// <remarks>
///     This class should be used with caution. It relies on <see cref="AsyncLocal{T}"/> which can have a negative performance
///     impact on async calls. It also creates a dependency on "ambient state" which can make testing more difficult.
/// </remarks>
[DebuggerDisplay("TenantContext = {TenantContext}")]
public class AsyncLocalTenantContextAccessor : ITenantContextAccessor
{
    private static readonly AsyncLocal<TenantContextHolder> _tenantContextCurrent = new();

    /// <inheritdoc />
    public TenantContext? TenantContext
    {
        get => _tenantContextCurrent.Value?.TenantContext;
        set
        {
            var holder = _tenantContextCurrent.Value;
            if (holder != null)
            {
                // Clear current HttpContext trapped in the AsyncLocals, as it's done.
                holder.TenantContext = null;
            }

            if (value != null)
            {
                // Use an object indirection to hold the HttpContext in the AsyncLocal,
                // so it can be cleared in all ExecutionContexts when it's cleared.
                _tenantContextCurrent.Value = new TenantContextHolder
                {
                    TenantContext = value
                };
            }
        }
    }

    private sealed class TenantContextHolder
    {
        public TenantContext? TenantContext;
    }
}
