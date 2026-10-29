# Troubleshooting

Start with the response or exception you see. The [quickstart](./quickstart.md) provides a small
working configuration to compare with your application.

## Registration and startup

| Symptom | Check |
| --- | --- |
| `AddMultiTenancy()` cannot be found | Install `Ayaka.MultiTenancy` and import `Ayaka.MultiTenancy`. The ASP.NET Core package alone does not include this extension. |
| `ConfigureRequestTenancy()` or `UseMultiTenancy()` cannot be found | Install `Ayaka.MultiTenancy.AspNetCore` and import `Ayaka.MultiTenancy.DependencyInjection`. |
| `AddTenantManagement()`, `UseInMemoryStore()`, or `UseHybridCache()` cannot be found | Install `Ayaka.MultiTenancy` and import `Ayaka.MultiTenancy.DependencyInjection`. |
| `UseMultiTenancy()` reports missing services | Call `AddMultiTenancy().ConfigureRequestTenancy(...)` before building the app. |
| Request options validation fails | Add at least one detection strategy. |
| Startup fails with `Unable to resolve service for type '…ITenantManager' while attempting to activate …` | `ResolveWithTenantManagement()` needs tenant management: add `AddTenantManagement()` and a store. |
| Resolving `ITenantManager` fails because `ITenantStore` is missing | Select `UseInMemoryStore()` or register your own singleton store. Management does not select one automatically. |
| `ILoggerFactory` cannot be resolved | Multi-tenancy requires logging. ASP.NET Core and the Generic Host register it; with any other host, register it yourself. |
| Cache options validation fails | Set `Expiration` between 1 minute and 1 hour, inclusive. |
| A keyed HybridCache cannot be resolved | Register `AddKeyedHybridCache(...)` with the same key you pass to `UseKeyedHybridCache(...)`. |
| `Cannot resolve scoped service '…' from root provider.` or `Cannot consume scoped service '…' from singleton '…'.` | Strategies, resolvers, and everything the tenant manager depends on are used as singletons. Use a factory such as `IDbContextFactory<T>`, or create a scope per operation. Both errors appear only with scope validation, which is on by default only in Development; in Production nothing fails, and the scoped service silently lives as long as the application. |

## An endpoint returns 400

No strategy returned an ID. Check the header name and value, or the host format. The host strategy
doesn't detect a tenant from `localhost`; use a subdomain such as `acme.localhost:5080` locally
(`*.localhost` resolves to your machine), or a header. Custom strategies should return `null` when
they cannot detect an ID. See [Tenant detection](./aspnetcore/detection.md).

For an endpoint that must work without a tenant, add `DisableMultiTenancyAttribute` as metadata.
For CORS preflight requests, put `UseCors()` before `UseMultiTenancy()`.

Routing can produce a matched **405 Method Not Allowed** endpoint. Tenant detection processes it
like any other matched endpoint, so a wrong-method request without a tenant may receive 400
before routing's 405 response is sent.

## An endpoint returns 404

With tenant-management resolution, check that the store contains the exact ID. The in-memory
store starts empty, matches case-sensitively, and loses data on restart. A custom resolver also
produces 404 when it returns `null`.

An unknown ID doesn't trigger fallback detection: if a header and host disagree, the first
configured strategy that returns an ID wins. See
[Combine strategies](./aspnetcore/detection.md#combine-strategies).

An ordinary unmatched route also produces 404, but the tenancy middleware skips it. With problem
details enabled, the title `Tenant is unknown.` identifies a tenant-resolution rejection.

## An endpoint returns 500

Exceptions from detection, resolution, or the tenant store propagate to your exception handling;
the middleware doesn't turn them into 400 or 404. If your exception handler re-executes the
pipeline, it runs detection and resolution again, so the same failure also breaks the error page.
Mark the error endpoint with `DisableMultiTenancyAttribute`. See
[Place the middleware](./aspnetcore/pipeline.md#place-the-middleware).

## An unknown tenant is accepted

The detect-only resolver accepts any detected ID and uses it as the display name. Select
`ResolveWithTenantManagement()` and configure a manager and store to require known tenants, or
use a custom resolver; see [Tenant resolution](./aspnetcore/resolution.md). Selecting a tenant
doesn't authorize the caller either; see [What Ayaka does not do](./index.md#what-ayaka-does-not-do).

## The tenant context is null

Check that `UseRouting()` runs before `UseMultiTenancy()`: placed before routing, the middleware
sees no endpoint and lets every request through without a tenant. Then check that an endpoint
matched, that it hasn't disabled multi-tenancy or short-circuited during routing, and that you read
the accessor after `UseMultiTenancy()` has run.

Work started from a request and not awaited, such as a fire-and-forget task or a timer callback,
sees `null` once the request has ended: the middleware clears the context then. Copy the tenant ID
into that work before you start it.

For background work, set the context where the job runs: queued work doesn't inherit it. See
[background work](./guides/background-work.md) and the tenant context's
[behavior and limits](./tenant-context.md#behavior-and-limits).

## The tenant was already set

The middleware throws `Tenant was already set previously in the request pipeline` when it
encounters an existing context on a tenant-enabled endpoint. Check for duplicate middleware
registration or earlier code setting the accessor. Do not manually set the context before the
middleware to influence detection; use a strategy or resolver instead. See
[Place the middleware](./aspnetcore/pipeline.md#place-the-middleware).

## Tenant data remains stale

Write through `ITenantManager` so successful changes evict the relevant entry. Direct database
updates bypass that path, and cached misses can hide newly added tenants until invalidation or
expiration. Check warning logs for failed eviction attempts and coordinate invalidation across
application instances when using local caches. See
[Caching](./management/caching.md#behavior-and-limits) and the
[cache sync guide](./guides/cache-sync.md).

## Problem details or activity tags are missing

Register `AddProblemDetails()` and use an `Accept` header supported by a registered problem-details
writer. Status codes are set even when no response body can be written.

The activity tag is written only after a successful resolution, and only when the request is traced,
for example by OpenTelemetry's ASP.NET Core instrumentation. Rejected and excluded requests don't
receive the tag. Enable Debug logs for `Ayaka.MultiTenancy.AspNetCore.RequestTenancyMiddleware` to
inspect detection and resolution; these logs can contain tenant IDs. See
[Problem details and tracing](./aspnetcore/pipeline.md#problem-details-and-tracing).
