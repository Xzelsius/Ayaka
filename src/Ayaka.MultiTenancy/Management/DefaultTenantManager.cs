// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.MultiTenancy.Management;

using Light.GuardClauses;
using Microsoft.Extensions.Logging;

/// <summary>
///     The default <see cref="ITenantManager" /> providing basic tenant management capabilities
///     using an <see cref="ITenantStore" /> as the underlying persistence.
/// </summary>
internal sealed partial class DefaultTenantManager : ITenantManager
{
    private readonly ITenantStore _store;
    private readonly ITenantCache? _cache;
    private readonly ILogger<DefaultTenantManager> _logger;
    private readonly Func<string, CancellationToken, ValueTask<Tenant?>> _loadFromStore;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DefaultTenantManager"/> class.
    /// </summary>
    /// <param name="store">The underlying <see cref="ITenantStore"/> to use.</param>
    /// <param name="cache">The optional <see cref="ITenantCache"/> to use for caching tenant lookups.</param>
    /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> used for logging.</param>
    public DefaultTenantManager(
        ITenantStore store,
        ITenantCache? cache,
        ILoggerFactory loggerFactory)
    {
        _store = store;
        _cache = cache;
        _logger = loggerFactory.CreateLogger<DefaultTenantManager>();

        // Created once, so cached lookups don't allocate a delegate per call
        _loadFromStore = (id, ct) => new ValueTask<Tenant?>(_store.GetAsync(id, ct));
    }

    /// <inheritdoc />
    public async ValueTask AddAsync(Tenant tenant, CancellationToken cancellationToken = default)
    {
        await _store.AddAsync(tenant.MustNotBeNull(), cancellationToken);
        await EvictSafelyAsync(tenant.Id);
    }

    /// <inheritdoc />
    public async ValueTask UpdateAsync(Tenant tenant, CancellationToken cancellationToken = default)
    {
        await _store.UpdateAsync(tenant.MustNotBeNull(), cancellationToken);
        await EvictSafelyAsync(tenant.Id);
    }

    /// <inheritdoc />
    public async ValueTask<bool> RemoveAsync(string id, CancellationToken cancellationToken = default)
    {
        var removed = await _store.RemoveAsync(id.MustNotBeNullOrWhiteSpace(), cancellationToken);
        await EvictSafelyAsync(id);

        return removed;
    }

    /// <inheritdoc />
    public ValueTask<Tenant?> GetAsync(string id, CancellationToken cancellationToken = default)
    {
        _ = id.MustNotBeNullOrWhiteSpace();

        return _cache is null
            ? new ValueTask<Tenant?>(_store.GetAsync(id, cancellationToken))
            : _cache.GetOrCreateAsync(id, _loadFromStore, cancellationToken);
    }

    /// <inheritdoc />
    public ValueTask<IReadOnlyList<Tenant>> GetAllAsync(CancellationToken cancellationToken = default)
        => new(_store.GetAllAsync(cancellationToken));

    private async ValueTask EvictSafelyAsync(string id)
    {
        if (_cache is null)
        {
            return;
        }

        try
        {
            // The store write already happened, so a caller cancelling now must not leave a stale entry behind
            await _cache.EvictAsync(id, CancellationToken.None);
        }
        catch (Exception exception)
        {
            TenantManagerLog.FailedToEvict(_logger, id, exception);
        }
    }

    private static partial class TenantManagerLog
    {
        [LoggerMessage(1, LogLevel.Warning, "Failed to evict tenant {TenantId} from the cache")]
        public static partial void FailedToEvict(ILogger logger, string tenantId, Exception exception);
    }
}
