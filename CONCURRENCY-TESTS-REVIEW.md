# Review: concurrency compliance tests (2026-09-28)

Branch: `feature/multi-tenancy-store-contract`. Two `Claude-TODO` comments asked for a review of the
concurrency tests in the compliance suites. Both TODOs are resolved; this note records the findings.

## Verdict

Both tests are sound. No logic changes were needed. The TODO comments were replaced with one-line
comments stating the purpose of the synchronization gates.

### `TenantCacheCompliance.Concurrent_requests_invoke_the_factory_once`

- The two-gate design (`factoryStarted` / `releaseFactory`) is what makes the test meaningful. Holding
  the factory open guarantees that no request can be served from a completed entry, so all 20
  requests must join the in-flight factory. Without it the test could pass trivially via cache hits.
- Mutant check: a cache without stampede protection (check → run factory → store) fails with
  `factoryCalls should be 1 but was 20` and passes every other cache test. This test is the only one
  that catches it.

### `TenantStoreCompliance.Allows_adding_many_tenants_parallel`

- The start gate (`TaskCompletionSource` with `RunContinuationsAsynchronously`, armed via
  `WaitAsync`) genuinely fans out: a metering store observed all 100 adds in flight at once.
  It is a real overlap test, not a sequential loop in disguise.
- Comparing the full expected set (`ShouldBe(expected, ignoreOrder: true)`) rather than only the count
  catches lost adds and corrupted entries alike.
- Mutant checks (each passed every other store test, only this test failed):
  - non-atomic copy-on-write add (`await Task.Yield()` between read and write): fails every run
  - plain `Dictionary<string, Tenant>`: fails 9 of 10 runs (inherent limit of a true race test;
    acceptable given the stated goal — it is neither a benchmark nor a conflict test)

## How it was verified

The test project does not currently compile (see below), so the two compliance suites plus the mutant
implementations were compiled in a throwaway project in the session scratchpad that imports
`test/Directory.Build.props`, references `src/Ayaka.MultiTenancy`, and uses `AssemblyName`
`Ayaka.MultiTenancy.Tests` to satisfy `InternalsVisibleTo`. All 20 real tests in
`HybridCacheTenantCacheTest` and `InMemoryTenantStoreTest` pass.

## Open item (not touched)

`test/Ayaka.MultiTenancy.Tests/Management/DefaultTenantManagerTest.cs` does not compile: it still
uses the old `DefaultTenantManager` constructor (now `(ITenantStore, ITenantCache?, ILoggerFactory)`)
and calls `RefreshAsync`, which no longer exists. Unrelated to the TODOs; needs updating before
`dotnet test test/Ayaka.MultiTenancy.Tests` works again.

## Files changed

- `test/Ayaka.MultiTenancy.Tests/Management/TenantCacheCompliance.cs` — TODO → intent comment
- `test/Ayaka.MultiTenancy.Tests/Management/TenantStoreCompliance.cs` — TODO → intent comment

Nothing committed.
