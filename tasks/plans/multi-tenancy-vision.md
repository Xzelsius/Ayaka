# Ayaka.MultiTenancy — Vision

> Status: draft, captured 2026-09-28 from Raphael's own description.
> This is the *vision* document. Individual features get their own plan file under `tasks/plans/`.

## Goal

`Ayaka.MultiTenancy` solves the **hard, real-life** problems of running a multi-tenanted .NET
application — not the per-tenant customisation problems. Concretely:

- Detect the tenant from an incoming HTTP request
- Open a tenant scope explicitly from anywhere (`await using var scope = await ...BeginScopeAsync("acme")`)
- Resolve a tenant-correct scoped `DbContext`
- Resolve scoped services that behave correctly inside a tenant context

It is deliberately **not** a Finbuckle.MultiTenant clone.

The `preview` suffix comes off once this vision is ~99% implemented.

## Non-goals (for now)

- Per-tenant service registrations / per-tenant DI containers
- Per-tenant `IOptions<T>` / configuration overlays / branding
- Tenant onboarding UI, billing, provisioning workflows
- Tenant lifecycle modelling (see D9)

## Architecture

```
+-------------------------------------------------------------+
|  Integration layer                                           |
|    Ayaka.MultiTenancy.AspNetCore  - detection + middleware   |
|    Ayaka.MultiTenancy.EntityFrameworkCore - DbContext hooks  |
|    (tenant scope: BeginScopeAsync)                           |
+-----------------------------+--------------------------------+
                              | resolves tenants only via
+-----------------------------v--------------------------------+
|  ITenantManager  - THE facade; nothing bypasses it           |
|    - read path used by detection, scopes, EF                 |
|    - write path for management                               |
|    - owns the caching policy                                 |
+-------------+-----------------------------+------------------+
              |                             |
      +-------v--------+          +---------v-------------+
      |  ITenantCache  |          |   ITenantStore        |
      |  (pluggable)   |          |   (pluggable)         |
      |                |          |                       |
      |  in-memory     |          |  in-memory            |
      |  Redis / SQL   |          |  configuration (RO)   |
      |  ...           |          |  DbContext            |
      +----------------+          |  Azure Table Storage  |
                                  |  ...or your own       |
                                  +-----------------------+
```

**Key invariant:** no part of an application talks to `ITenantStore` directly. `ITenantManager` is
the only entry point, and it is what makes the storage layer swappable and the cache transparent.

## Planned package topology

| Package                                         | Purpose                                                                     | Status  |
|-------------------------------------------------|-----------------------------------------------------------------------------|---------|
| `Ayaka.MultiTenancy.Abstractions`               | contracts: context, accessor, tenant, store, cache, manager                 | exists  |
| `Ayaka.MultiTenancy`                            | defaults: AsyncLocal accessor, manager, in-memory store, cache, tenant scope | exists |
| `Ayaka.MultiTenancy.AspNetCore`                 | detection strategies, middleware, request policies                          | exists  |
| `Ayaka.MultiTenancy.EntityFrameworkCore`        | tenant-aware `DbContext` hookup points                                      | planned |
| `Ayaka.MultiTenancy.Stores.EntityFrameworkCore` | `ITenantStore` backed by a `DbContext`                                      | planned |
| `Ayaka.MultiTenancy.Stores.AzureTables`         | `ITenantStore` backed by Azure Table Storage                                | planned |

The configuration-section store lives in `Ayaka.MultiTenancy` itself. No caching packages are
planned — see Q6.

## Decisions

Answered 2026-09-28. Referenced by the question numbers they resolve.

### D1 (Q1) — `BeginScopeAsync` creates a fresh dependency scope

In order:

1. Create a fresh DI scope
2. Resolve `ITenantManager` **from that scope**
3. Validate that the tenant exists
4. Assign `ITenantContextAccessor.TenantContext`
5. Set the tenant tag on the current `Activity`, like the middleware does

Long term it should also open a logging scope. The middleware does not do that today either, so
there is no hurry — but both paths should gain it together.

The returned scope exposes its `IServiceProvider`, so `scope.ServiceProvider.GetRequiredService<T>()`
yields tenant-correct scoped services (this is the `DbContext` story).

Note the asymmetry with ASP.NET Core: the middleware must **not** create a DI scope (the request
scope already exists); it only resolves + assigns. The resolve-and-project logic is shared, the
scope creation is not.

### D2 (Q3) — Async only

No sync overload for now. A sync variant would imply skipping validation; parked rather than
designed away.

### D3 (Q4) — `TenantContext` is derived from `Tenant`, not the same object

`Tenant` is the management-side object; `TenantContext` is the deliberately simplified ambient
variant, so management may hold more detail than what belongs in the `AsyncLocal`. See D8 for how
the derivation happens.

### D4 (Q5) — No read-only store interface

A store that cannot write throws. The configuration-section store allows lookups only. No
`IWritableTenantStore` split, no capability flags — keep `ITenantStore` as one interface.

### D5 (Q7) — A store is not a hard requirement

`AddMultiTenancy()` / `ITenantContextAccessor` stay usable standalone for someone driving the tenant
context with their own mechanism. But **the middleware and `BeginScopeAsync` both require
`ITenantManager`**, and therefore a store.

Correction to note: today the middleware does *not* touch `ITenantManager` at all — it assigns
`new TenantContext(id, id)` straight from the detected string. Making the manager a hard dependency
of `UseMultiTenancy()` is a behaviour change (acceptable while in preview), and
`ApplicationBuilderExtensions.VerifyServiceAreRegistered` has to be extended to check for it.

### D6 (Q2) — The scope restores the previous context on dispose

`BeginScopeAsync` sets the tenant, and disposal resets to whatever was there before. Nesting is
therefore legal and behaves like a stack. The middleware keeps its own "already set → throw" policy.

Mechanically this falls out of the holder pattern for free, and does **not** hit the
`ExecutionContext` problem below: dispose mutates the holder that is already in the caller's
execution context (`holder.TenantContext = previous`) rather than writing the `AsyncLocal` slot, so
`DisposeAsync` may be a normal `async` method.

### D7 (Q13) — Unsupported writes throw `NotSupportedException`

That is what the BCL uses for "this implementation does not support this operation".
`TenantManagementException` stays for genuine management failures (duplicate id, missing tenant).

### D8 (Q12) — Project, do not copy raw

`TenantContext` is produced by a projection seam (`ITenantContextFactory` or an equivalent delegate)
so the ambient object never hands out a reference through which someone could mutate what the
manager or the cache is holding.

Worth knowing while designing it: with the payload immutable all the way down, sharing references is
in fact safe — `string` is immutable and the attributes are already an immutable dictionary. The
gap is that `Tenant.Attributes` is typed as the **interface** `IImmutableDictionary<string, string>`,
which only *declares* immutability; a pathological implementation could violate it. Tightening the
projected type (`ImmutableDictionary<string, string>`, or `FrozenDictionary<string, string>` — built
once, read on every request, which is exactly the right trade for this object) makes the guarantee
real and lets the projection be allocation-free on the hot path.

Follow-on: cache the projected `TenantContext` alongside the `Tenant`, so projection runs once per
cache fill rather than once per request.

### D9 (Q9) — No `IsActive` on `Tenant`

Asked what it was for, and the honest answer is: only to let the middleware answer "known but
suspended" differently from "unknown". That is application policy, not plumbing — different apps
want different lifecycle models (trial / active / suspended / archived), and a pluggable store
already lets an app keep whatever columns it wants.

Instead, slice 3 gets a **policy hook** on the request path ("may this resolved tenant proceed?"),
so suspension can be implemented from `Attributes` without the core model committing to one
lifecycle. Promote a first-class property later only if a common shape emerges.

### D10 (Q11) — Lift the activity tag name into the core

`ActivityTagName` currently sits on `RequestTenancyOptions` in the AspNetCore package. Since D1 has
the core tenant scope tagging activities too, it moves to a core options type that both read. This
is forced by slice 2, not "eventually".

Still undecided and cheap to change now: the tag is called `tenant`; OpenTelemetry naming convention
would suggest `tenant.id`.

Related wrinkle: the middleware deliberately uses `IHttpActivityFeature` rather than
`Activity.Current`. Outside HTTP only `Activity.Current` exists, so the two paths tag differently.

## Verified runtime constraints

### `AsyncLocal` writes do not escape an `async` method

Probed on net10, 2026-09-28. D1 + D2 together mean the ambient assignment happens *after* an
`await` inside `BeginScopeAsync`. That does not work:

| Shape                                                          | Caller sees |
|----------------------------------------------------------------|-------------|
| `async Task BeginScopeAsync` assigning the accessor after `await` | `null`      |
| ... same, but the awaited work completed synchronously (cache hit) | `null`    |
| non-async prologue pushes the holder, async continuation fills it | **value**   |
| middleware shape (assign after `await`, call `_next` inline)     | **value**   |
| holder shape across 3 concurrent flows                           | no bleed    |

`AsyncMethodBuilderCore.Start` restores the caller's `ExecutionContext` after the synchronous
portion of every `async` method, so the result is the same whether the awaited work suspends or
not — there is no "works by accident on a cache hit" trap, which at least makes it predictable.
The middleware works today only because it awaits `_next(context)` from *inside* the same method,
so downstream is a descendant execution context.

**Consequence for D1/D2:** the entry point must be a **non-async** method that pushes the holder
onto the caller's execution context synchronously, then returns an async continuation that resolves
the tenant and mutates the holder:

```csharp
public ValueTask<ITenantScope> BeginScopeAsync(string tenantId, CancellationToken ct = default)
{
    var holder = _accessor.Push();               // sync: writes the CALLER's ExecutionContext
    return CompleteAsync(holder, tenantId, ct);  // async: resolve, validate, fill the holder
}
```

Two follow-ons:

- `AsyncLocalTenantContextAccessor` needs a seam for this. Its setter always allocates a fresh
  holder and offers no way to pre-push an empty one.
- If resolution fails (unknown tenant), the pushed holder must be restored to the previous value
  before the exception propagates, or the caller is left holding a live-but-empty holder.

Disposal is unaffected — see D6.

See also Q10 — putting the context in the DI scope sidesteps this entirely for the `DbContext` path.

## Delta from today

What the vision requires that does not exist yet:

1. `ITenantStore` has no point lookup (`GetAsync(id)`). `DefaultTenantManager.GetAsync` calls
   `GetAllAsync()` and does a `FirstOrDefault` — unusable for a DB- or table-storage-backed store.
2. No cache layer at all. `ITenantManager` is currently a pass-through to the store.
3. `ITenantManager` has no `UpdateAsync` even though `ITenantStore` does.
4. Store error semantics are inconsistent: `AddAsync` throws on duplicate, `UpdateAsync` throws on
   missing, `RemoveAsync` silently ignores missing. Per D4/D7 an unsupported write must also throw,
   so the whole error contract needs writing down.
5. No tenant scope primitive, and no accessor seam to implement one correctly (see above).
6. The middleware never consults `ITenantManager` — an unknown tenant sails straight through
   (GitHub #150), and there is no "block on missing tenant" policy and no tenant policy hook (D9).
7. `TenantContext` cannot carry attributes, and there is no projection seam (D8).
8. No EF Core integration of any kind.
9. Detection strategies are instantiated from the **root** provider inside
   `AddOptions<RequestTenancyOptions>().Configure<IServiceProvider>(...)`
   (`src/Ayaka.MultiTenancy.AspNetCore/DependencyInjection/RequestTenancyBuilder.cs:131`).
   `IOptions<T>` is a singleton, so a claims- or route-based strategy that constructor-injects a
   scoped service becomes a captive dependency. This lifetime model needs rethinking before
   GitHub #179 / #180 are implemented.
10. The activity tag name has to move to the core (D10).

## Feature slices

Roughly in dependency order. Each gets its own plan file when started.

1. **Storage & caching core** — `ITenantStore` point lookup + written-down error contract,
   `ITenantCache`, `ITenantManager` as caching facade, `UpdateAsync`. *Blocks everything else.*
2. **Tenant scope** — `ITenantScope` + `BeginScopeAsync` with the non-async prologue, accessor
   holder seam, restore-on-dispose, activity tagging (forces D10).
3. **Resolved tenant context** — projection seam (D8); middleware resolves through `ITenantManager`;
   missing / unknown tenant policies plus the tenant policy hook (GitHub #150, D9).
4. **EF Core hookup points** — query filters, tenant stamping, connection routing. *Postponed for
   design (Q8), not for priority.*
5. **Additional stores** — configuration section (read-only), EF Core, Azure Table Storage.
6. **Additional detection strategies** — claims (GitHub #179), route (GitHub #180); requires the
   strategy-lifetime fix from delta #9.
7. **Docs, samples, PACKAGE.md** — currently `Ayaka.MultiTenancy/PACKAGE.md` says "Soon&trade;" and
   `Ayaka.MultiTenancy.AspNetCore/PACKAGE.md` has three empty sections. Then drop `preview`.

## Open questions

### Q6 — `ITenantCache` seam, or a bare `HybridCache` dependency?

- (a) No seam: `DefaultTenantManager` depends on `HybridCache` directly
- (b) `ITenantCache` in Abstractions, with a single `HybridCache`-backed default implementation

**Lean: (b).** `HybridCache` is L1-only in-memory when no `IDistributedCache` is registered, and
becomes L1+L2 the moment someone calls `AddStackExchangeRedisCache()`. So **one** implementation
covers the in-memory default *and* Redis / SQL / Garnet, and Ayaka never ships a caching package.
Only one new dependency (`Microsoft.Extensions.Caching.Hybrid`, to be added to `eng/Packages.props`).

The seam still earns its place because caching *tenants* is not generic caching:

- negative caching — an unknown tenant id must not hit the store on every request (also a cheap DoS
  vector otherwise)
- invalidation — manager writes must evict; `HybridCache` tags help here
- "load them all" — tenant sets are small and change rarely, so an eager whole-set cache with a
  change token is a legitimate strategy someone will want, and it cannot be expressed as
  `GetOrCreate(id)`

So the interface should be domain-shaped, roughly `GetOrCreateAsync(id, factory)` / `EvictAsync(id)`
/ `EvictAllAsync()`, not a generic key-value cache.

Risk to check when building it: `Tenant.Attributes` is `IImmutableDictionary<string, string>`, and
L2 requires serialization. Needs verifying that `System.Text.Json` round-trips the declared
interface type, or the projected type gets tightened anyway per D8.

### Q8 — EF Core: which isolation pattern first, one package or two?

**Postponed** by decision. Prior lean, for when it is picked up: two packages (tenant-aware
`DbContext` hookups vs. an EF-backed `ITenantStore` — different audiences, different dependencies);
shared-database-with-discriminator first, database-per-tenant second.

### Q10 — Where does the ambient context actually live?

`BeginScopeAsync` creates a DI scope anyway (D1). If `ITenantContextAccessor` were **scope-resident**
rather than `AsyncLocal`-backed, the `DbContext` path would work with no execution-context games at
all — `scope.ServiceProvider.GetRequiredService<MyDbContext>()` simply gets the scoped accessor.

- (a) `AsyncLocal` only, with the holder prologue described above
- (b) scope-resident, `AsyncLocal` only as a fallback for code that cannot reach `scope.ServiceProvider`
- (c) both, with a documented precedence

**Lean:** (a) for now — it keeps one source of truth and the holder prologue is a contained trick —
but (b) is worth a serious look before slice 2 is built, because it removes a whole class of subtle
bugs for the exact scenario that motivated `BeginScope` in the first place.
