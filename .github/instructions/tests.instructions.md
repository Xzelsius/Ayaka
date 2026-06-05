---
applyTo: "test/**/*.cs"
---

<!-- Derived from AGENTS.md and .claude/rules/ — Copilot code review reads only this tree; re-sync after changing those. -->

# test/ — xunit test projects

Frameworks & layout:

- xunit + FluentAssertions + FakeItEasy — all available via global usings (no `using Xunit;` etc.
  needed); versions are managed centrally in `eng/Packages.props`
- Test projects live in `test/<Project>.Tests` (not every package has one); namespaces mirror the
  source project with a `.Tests` suffix
- The file header `// Copyright (c) Raphael Strotz. All rights reserved.` applies here too

Naming:

- Test class per type under test, named `<TypeName>Test` (e.g. `AsyncLocalTenantContextAccessorTest`)
- Group scenarios with nested classes named for the member or situation under test (e.g.
  `AddMultiTenancy`, `When`, `WhenNotNull`)
- Method names are readable snake_case sentences:
  `Getting_TenantContext_returns_null_when_no_TenantContext_is_set`

Writing tests:

- Use `[Fact]`, or `[Theory]` + `[InlineData]` for parameterized cases
- Assert with FluentAssertions (`result.Should().BeSameAs(expected)`)
- Fake with FakeItEasy: `A.Fake<T>()` to create, `A.CallTo(() => ...)` to configure and assert
- Internals can be made testable via `InternalsVisibleTo` in the src `.csproj` (already declared
  for `Ayaka.Nuke` and `Ayaka.MultiTenancy`)

Workflow:

- Run a single project fast: `dotnet test test/<Project>.Tests --filter "FullyQualifiedName~<TestClass>"`
- When changing existing behavior in `src/`, update the matching test file rather than adding
  parallel new ones
