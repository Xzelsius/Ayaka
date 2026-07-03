---
paths:
  - "test/**/*.cs"
---

# test/ — additions to AGENTS.md

The full guide is in `AGENTS.md` (always loaded); this file only adds test-specific detail.

- Use `[Fact]`, or `[Theory]` + `[InlineData]` for parameterized cases
- Pass `TestContext.Current.CancellationToken` to methods that accept a `CancellationToken` — the xUnit1051 analyzer flags calls that omit it
- Assert with Shouldly: `actual.ShouldBe(expected)`, `actual.ShouldNotBeNull()`, `actual.ShouldBeOfType<T>()`, collections via `ShouldHaveSingleItem()` / `ShouldContain(...)`
- `ShouldHaveSingleItem()` and `ShouldBeOfType<T>()` return the (typed) value — chain on it instead of re-indexing or casting
- Deep structural equivalence: `actual.ShouldBeEquivalentTo(expected)` — use where record equality is wrong because a member (e.g. an `ImmutableDictionary`) compares by reference
- Exception assertions use Shouldly's static form: `await Should.ThrowAsync<T>(act)` / `Should.Throw<T>(action)` (the delegate is the argument, not the receiver)
- Fakes: `A.Fake<T>()` to create, `A.CallTo(() => ...)` to configure and assert (FakeItEasy)
- Internals can be made testable via `InternalsVisibleTo` in the src `.csproj` (already declared
  for `Ayaka.Nuke` and `Ayaka.MultiTenancy`)
- When changing existing behavior in `src/`, extend the matching existing test file — don't create
  a parallel new one
