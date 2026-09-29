# Slice 1 — Storage & caching core

> Part of [multi-tenancy-vision.md](./multi-tenancy-vision.md). Blocks every other slice.
> Status: planned, 2026-09-28.

## Goal

Turn `ITenantManager` into the caching façade the vision calls for: the single entry point every
part of an application uses, sitting on a pluggable `ITenantStore` and a pluggable `ITenantCache`.

Nothing outside this slice changes — `Ayaka.MultiTenancy.AspNetCore` does not reference
`ITenantManager` today, so the blast radius is `Abstractions` + `Ayaka.MultiTenancy` + their tests.

## In scope

- `ITenantStore`: point lookup, written-down error contract
- `ITenantCache`: new abstraction + default implementation
- `ITenantManager`: `UpdateAsync`, `RefreshAsync`, cache wiring
- `InMemoryTenantStore`: conform to the new contract
- DI builder surface for choosing a store and a cache
- Tests, Public API files, docs

## Out of scope

- `TenantContext` projection (slice 3)
- Middleware changes (slice 3)
- Configuration / EF / Azure Table stores (slice 5) — but the contract must make them writable *later*
  without another breaking change

## Step 0 — verified against upstream (2026-09-28)

Checked against `dotnet/extensions` source per the repo convention, not the local NuGet cache.

1. **`RemoveByTagAsync` is implemented.** `Internal/DefaultHybridCache.TagInvalidation.cs` carries a
   real implementation with tag-expiry tracking — not a declared-but-inert API. `EvictAllAsync` via
   tags is safe.
2. **Immutability decides whether every cache hit deserializes.**
   `Internal/ImmutableTypeCache.IsTypeImmutable` returns `true` only for `string`, or for a **value
   type or sealed class** that also carries `[ImmutableObject(true)]`. Interfaces and non-sealed
   types are always treated as mutable, and `Internal/DefaultHybridCache.MutableCacheItem.TryGetValue`
   calls `serializer.Deserialize(...)` on **every L1 read**.

   `Tenant` is an unsealed `record`, so as designed today every cache hit would cost a JSON
   round-trip. See "Immutability" below — this is what makes caching cheap enough that an in-memory
   store does not need special-casing.

Still to confirm when implementing: the exact `GetOrCreateAsync<TState, T>` parameter order, and the
current `Microsoft.Extensions.Caching.Hybrid` version to pin in `eng/Packages.props`.

## Needs approval before implementing

**New dependency:** `Microsoft.Extensions.Caching.Hybrid` on `Ayaka.MultiTenancy` (not on
`Abstractions` — the interface stays dependency-free). This is the entire caching story: with no
`IDistributedCache` registered `HybridCache` is an in-memory L1 cache, and it becomes L1+L2 the
moment someone calls `AddStackExchangeRedisCache()`. One dependency instead of a family of
`Ayaka.MultiTenancy.Caching.*` packages.

## Design

### Async return types

| Interface       | Returns     | Why                                                                              |
|-----------------|-------------|----------------------------------------------------------------------------------|
| `ITenantManager` | `ValueTask` | Hot path — called per request; cache hits complete synchronously                  |
| `ITenantCache`   | `ValueTask` | Hot path, and mirrors `HybridCache`'s own shape                                   |
| `ITenantStore`   | `Task`      | Cold path (cache miss / management only), and it is the interface **third parties implement most** — `Task` has no await-once footgun |

This is a deliberate split, not an oversight. Document the rule on the interfaces themselves so the
next person does not "fix" the inconsistency.

### `ITenantStore`

```csharp
public interface ITenantStore
{
    Task<Tenant?> GetAsync(string id, CancellationToken cancellationToken = default);      // NEW
    Task<IReadOnlyList<Tenant>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Tenant tenant, CancellationToken cancellationToken = default);
    Task UpdateAsync(Tenant tenant, CancellationToken cancellationToken = default);
    Task<bool> RemoveAsync(string id, CancellationToken cancellationToken = default);   // CHANGED
}
```

`GetAsync` is the important addition: today `DefaultTenantManager.GetAsync` calls `GetAllAsync()` and
does a `FirstOrDefault`, which is fine in memory and hopeless against Azure Tables or a database.

### Error contract

Written down because three implementations will have to honour it, and today's is accidental.

| Operation     | Condition            | Behaviour                          |
|---------------|----------------------|------------------------------------|
| `GetAsync`    | id unknown           | return `null` — **not** an exception |
| `GetAllAsync` | store empty          | empty list, never `null`           |
| `AddAsync`    | id already exists    | throw `TenantManagementException`  |
| `UpdateAsync` | id does not exist    | throw `TenantManagementException`  |
| `RemoveAsync` | id does not exist    | return `false` — **not** an exception |
| `RemoveAsync` | removed              | return `true`                      |
| any write     | store is read-only   | throw `NotSupportedException` (D7) |
| any           | `null`/blank id, `null` tenant | `ArgumentNullException` / `ArgumentException` |

The add-throws / remove-returns-`bool` split is the BCL's own convention (`Dictionary.Add` throws on
a duplicate, `Dictionary.Remove` returns `bool` and never throws), and it gives idempotent-delete
semantics for free: an at-least-once "tenant offboarded" message redelivering does not blow up,
while a typo'd id is still visible in the return value. Today's implementation discards
`TryRemove`'s result, which is the one option with no upside.

Argument validation happens at `ITenantManager` (the façade everyone calls) and at the entry points
of the store implementations Ayaka ships. Store implementers may assume non-null. Use
`Light.GuardClauses` — it is already referenced by every non-test project but currently used nowhere
in `src/`, so this slice establishes the convention.

### `ITenantCache`

```csharp
public interface ITenantCache
{
    /// <remarks>A <c>null</c> result is a cacheable outcome — implementations SHOULD cache misses.</remarks>
    ValueTask<Tenant?> GetOrCreateAsync(
        string id,
        Func<string, CancellationToken, ValueTask<Tenant?>> factory,
        CancellationToken cancellationToken = default);

    ValueTask EvictAsync(string id, CancellationToken cancellationToken = default);

    ValueTask EvictAllAsync(CancellationToken cancellationToken = default);
}
```

Three members, deliberately domain-shaped rather than a generic key-value cache:

- **Negative caching is part of the contract.** An unknown tenant id must not reach the store on
  every request — otherwise spraying random `X-Tenant` headers is a free way to hammer the database.
- **`EvictAllAsync`** exists so manager writes and bulk changes have one invalidation call.
- The factory takes `id` as a parameter rather than closing over it. `DefaultTenantManager` builds
  the delegate **once in its constructor** (`_loadFromStore = (id, ct) => _store.GetAsync(id, ct)`),
  so the hot path allocates nothing and the interface stays non-generic.

`GetAllAsync` is **not** cached in this slice — it is a listing/admin operation, and caching it drags
in list-invalidation on every write for little gain. Documented as a known limitation.

### `ITenantManager`

```csharp
public interface ITenantManager
{
    ValueTask<Tenant?> GetAsync(string id, CancellationToken cancellationToken = default);
    ValueTask<IReadOnlyList<Tenant>> GetAllAsync(CancellationToken cancellationToken = default);
    ValueTask AddAsync(Tenant tenant, CancellationToken cancellationToken = default);
    ValueTask UpdateAsync(Tenant tenant, CancellationToken cancellationToken = default);   // NEW
    ValueTask<bool> RemoveAsync(string id, CancellationToken cancellationToken = default); // CHANGED
    ValueTask RefreshAsync(string id, CancellationToken cancellationToken = default);      // NEW
}
```

`DefaultTenantManager` behaviour:

- `GetAsync` → `_cache.GetOrCreateAsync(id, _loadFromStore, ct)`
- `GetAllAsync` → straight to the store, uncached
- `AddAsync` / `UpdateAsync` / `RemoveAsync` → **store first, then `EvictAsync(id)`**. Store failure
  must not evict; evict failure leaves a stale entry, which is logged, not thrown.
  `RemoveAsync` evicts even when the store returns `false` — a stale positive or negative entry can
  exist regardless of whether the store held the tenant — and passes the `bool` straight through.
- `RefreshAsync` → `_cache.EvictAsync(id)`. The explicit escape hatch for out-of-band changes (another
  process wrote to a shared store).

Evict rather than repopulate on write: the store may normalise or reject what was written, and a
negative entry may already exist for that id.

### `HybridCacheTenantCache`

```csharp
internal sealed class HybridCacheTenantCache : ITenantCache
{
    private const string KeyPrefix = "ayaka:tenant:";
    private const string TenantsTag = "ayaka:tenants";
    private static readonly string[] Tags = [TenantsTag];

    public ValueTask<Tenant?> GetOrCreateAsync(string id, Func<string, CancellationToken, ValueTask<Tenant?>> factory, CancellationToken ct = default)
        => // _cache.GetOrCreateAsync(KeyPrefix + id, (id, factory),
           //     static async (s, t) => new TenantCacheEntry(await s.factory(s.id, t)),
           //     _entryOptions, Tags, ct)  ->  .Tenant
}

internal sealed record TenantCacheEntry(Tenant? Tenant);
```

Two details that matter:

- **The `TenantCacheEntry` wrapper** means the cached value is never `null`, so negative caching does
  not depend on `HybridCache` and its serializer handling a null `T`.
- **The `(id, factory)` tuple as `TState`** keeps the factory `static` and the call allocation-free.

Key prefix and tag are namespaced so Ayaka never collides with the application's own `HybridCache`
usage — it is a shared instance.

`TenantCacheOptions` carries one global expiration. `HybridCache.GetOrCreateAsync` takes its options
*before* the factory runs, so hits and misses could not have different TTLs anyway — accepted
(S1-B).

### Immutability — required, not an optimisation

Per step 0, `HybridCache` deserializes on every read for any type it cannot prove immutable, and
proof means *sealed (or a value type) plus `[ImmutableObject(true)]`*.

- `TenantCacheEntry` must be a **sealed record with `[ImmutableObject(true)]`**. It is the type
  `HybridCache` actually sees, so this is what flips the read path from "deserialize per hit" to
  "return the reference".
- `Tenant` should be **sealed** too. `[ImmutableObject(true)]` is a trust statement — upstream's own
  comment says as much — and it is only honest if the graph underneath really is immutable.
  `Tenant.Attributes` is typed as the *interface* `IImmutableDictionary<string, string>`, which only
  declares immutability. Tightening it (per vision D8) makes the claim true.

This is what answers "an in-memory store does not benefit from caching": with the marking, an L1 hit
is a reference return, so caching `InMemoryTenantStore` costs a key concat and a dictionary lookup
rather than a JSON round-trip. Not worth special-casing. Without the marking it would be a clear
pessimisation.

Cost of sealing `Tenant`: nobody can derive `record MyTenant : Tenant` to add typed fields. That is
what `Attributes` is for, and a derived type would not survive the slice-3 projection anyway — but
it is a real door being closed (S1-D).

### DI surface

```csharp
services.AddMultiTenancy()
        .AddTenantManagement()      // registers ITenantManager + default cache
        .UseInMemoryStore()
        .ConfigureCache(o => o.Expiration = TimeSpan.FromMinutes(5));
```

| Method                         | Effect                                                    |
|--------------------------------|-----------------------------------------------------------|
| `UseInMemoryStore()`           | exists                                                    |
| `UseStore<TStore>()`           | NEW — generic escape hatch for a custom store             |
| `UseCache<TCache>()`           | NEW — replace the cache implementation                    |
| `ConfigureCache(Action<TenantCacheOptions>)` | NEW                                         |
| `WithoutCaching()`             | NEW — `services.RemoveAll<ITenantCache>()`                |

Default when nothing is said: `HybridCacheTenantCache`, which is an **in-memory** cache until the
application registers an `IDistributedCache`. The default registration also calls
`services.AddHybridCache()`.

**No `NullTenantCache` type ships.** `DefaultTenantManager` takes `ITenantCache?` and goes straight
to the store when none is registered, so `WithoutCaching()` is just a de-registration. One less
public type, one predictable branch, and the off switch still exists.

**Fix while here:** `UseInMemoryStore()` uses `TryAddSingleton`, so first-call-wins and a later
`UseStore<T>()` would silently do nothing. The builder methods should `RemoveAll<T>()` then `Add`, so
last-call-wins is predictable. Applies to the cache methods too.

## Known limitations to document

- `GetAllAsync` is uncached.
- **Cross-node L1 invalidation.** With a shared store and multiple instances, instance A's write
  evicts A's L1 and the shared L2, but not B's L1. B serves a stale tenant until its entry expires.
  Document it and point at `ITenantCache.EvictAsync` plus a short expiration as the mitigation
  (`RefreshAsync` was dropped, see S1-E). `HybridCache` has no backplane, so this stays true until
  one is added.
- Hit and miss share one TTL — `HybridCache` cannot vary entry options by result.
- Evictions discard loads that are still in progress on the **same** instance only (S1-F).

## Work breakdown

1. Step 0 verification (above); pin `Microsoft.Extensions.Caching.Hybrid` in `eng/Packages.props`
2. `Abstractions`: `ITenantStore.GetAsync`, XML docs carrying the error contract, `ITenantCache`,
   `ITenantManager` reshape
3. `Ayaka.MultiTenancy`: `InMemoryTenantStore` (`GetAsync`; `RemoveAsync` just returns what
   `TryRemove` already gives it; replace the throwing-`AddOrUpdate` in `UpdateAsync` with an
   explicit compare-and-swap loop), seal `Tenant` + `[ImmutableObject(true)]`,
   `HybridCacheTenantCache`, `TenantCacheOptions`, `DefaultTenantManager` rewrite (nullable cache)
4. DI builder methods + the `TryAdd` → `RemoveAll`+`Add` fix
5. Tests (below)
6. `PublicAPI.Unshipped.txt` for both projects — every changed signature needs a `*REMOVED*` line
   plus the new line. Never touch `PublicAPI.Shipped.txt`
7. Docs + `PACKAGE.md`
8. `./build.sh` (`build.cmd` on Windows) before declaring done — not just `dotnet build`

## Tests

Extend the existing files rather than creating parallel ones, per `.claude/rules/tests.md`.

- **`TenantStoreCompliance<TFixture>`** — extend with `GetAsync` cases and the full error contract
  (add-duplicate throws, update-missing throws, remove-missing returns `false`, remove-existing
  returns `true`, get-missing returns `null`).
  Every store Ayaka ever ships inherits these, which is exactly what this base class was built for.
- **`TenantCacheCompliance<TFixture>`** — new, mirroring the store compliance pattern:
  caches a hit; caches a miss (factory invoked once for a repeated unknown id); `EvictAsync` forces
  a reload; `EvictAllAsync` clears everything; concurrent `GetOrCreateAsync` calls the factory once
  (stampede protection).
- **`InMemoryTenantStoreTest`** — keeps its instance-isolation test, inherits the extended compliance.
- **`HybridCacheTenantCacheTest`** — via the new compliance base. Add one test asserting the
  immutable read path: the same reference comes back from two consecutive hits (this is what
  silently regresses if someone un-seals the type or drops the attribute).
- **`DefaultTenantManagerTest`** — with FakeItEasy: reads go through the cache; each write calls the
  store *then* evicts; a throwing store does not evict; `RefreshAsync` evicts; **with no
  `ITenantCache` registered, reads go straight to the store**.
- **DI tests** — `TenantManagementBuilderExtensions` for the new methods, including that a later
  `UseStore<T>()` / `UseCache<T>()` actually wins.

Conventions: `TestContext.Current.CancellationToken` everywhere, Shouldly's static
`await Should.ThrowAsync<T>(act)` form, snake_case sentence names, nested classes per member.

## Commits & PR

- Conventional commits, and **not** `!` — a `!` bumps the major version for the whole repo including
  the stable `Ayaka.Nuke`. `.claude/rules/source.md` reserves that for real `Ayaka.Nuke` breaks; the
  preview packages break under a plain `feat:`.
- Suggested split: `feat: add tenant point lookup and store error contract` →
  `feat: add tenant cache abstraction` → `feat: make tenant manager a caching facade` →
  `docs: document tenant storage and caching`
- PR labels drive the changelog: `enhancement`, plus `breaking-change` if the preview API break
  should be called out in the release notes.

## Decisions inside this slice

All resolved — the slice is fully specified and ready to implement on approval.

- **S1-A — `RemoveAsync` returns `bool`.** Accepted 2026-09-28, following the BCL's
  add-throws / remove-returns-`bool` convention.
- **S1-D — `Tenant` is sealed.** Accepted 2026-09-28: the framework is not designed for anyone to
  write their own tenant object. Extension happens through `Attributes` and through custom
  `ITenantStore` implementations, which map *their* storage model onto `Tenant` rather than deriving
  from it — which is the right shape for the EF and Azure Table stores in slice 5 anyway.
- **S1-B — one global TTL.** Accepted 2026-09-28.
- **S1-C — caching on by default**, as an in-memory cache (`HybridCache` with no `IDistributedCache`
  registered). No no-op cache type; `WithoutCaching()` de-registers instead. Accepted 2026-09-28.

### Implementation review follow-ups (2026-09-29)

These supersede the parts of the design above that they contradict.

- **S1-E — no `RefreshAsync`.** Out-of-band changes are invalidated through `ITenantCache.EvictAsync`
  directly; a manager method would not reach other nodes' local caches anyway.
- **S1-F — evictions discard in-progress loads.** `HybridCache` stores a load's result even when the
  key was removed (or its tag invalidated) while the load ran. Entries now carry their load-start
  time; an entry loaded before the latest eviction of its tenant is dropped and reloaded (max 3
  attempts). Deliberately single-instance — no attempt to solve multi-node invalidation here.
- **S1-G — `Expiration` must be between 1 minute and 1 hour**, validated on host start.
- **S1-H — Public API files for the preview packages** may be maintained loosely (editing
  `PublicAPI.Shipped.txt` directly instead of `*REMOVED*` bookkeeping). `AGENTS.md` still says
  otherwise — align it if this stays the rule.
- **Writes evict with `CancellationToken.None`** once the store write has succeeded, so a cancelled
  caller cannot leave a stale entry behind.

### Open

- **Tenant id pattern** (slice 3): ids not matching a defined pattern are rejected before any lookup;
  a valid id is always looked up and a miss is cached. Also keeps ids within `HybridCache`'s key
  limits — ids over ~1000 characters or with control characters currently bypass the cache.
- **Optional dependencies of `DefaultTenantManager`**: `ITenantCache?` without a default value is still
  required by the container, so `WithoutCaching()` would break resolution; `ILoggerFactory` is
  required too. Proposed: `ITenantCache? cache = null, ILogger<DefaultTenantManager>? logger = null`.
- Still to do from the work breakdown: DI builder methods (`UseStore`, `UseCache`, `ConfigureCache`,
  `WithoutCaching`, `TryAdd` → `RemoveAll`+`Add`), docs + `PACKAGE.md`.
