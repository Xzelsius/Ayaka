# Coding Guidelines

Ayaka has several coding guidelines one should follow when contributing to the project. These guidelines are here to ensure that the codebase is consistent and maintainable.

Most of them are based on Visual Studio's default settings, but some are based on my style of coding.

## EditorConfig

Ayaka uses an `.editorconfig` file that covers most of the coding style.

This file is automatically used by Visual Studio and other editors to ensure that the code is formatted correctly.

::: info
The `.editorconfig` isn't flawless, but should give you a good starting point to what I expect.
:::

## Code Style

### File Headers

The following file header should be present in every C# code file:

```csharp
// Copyright (c) Raphael Strotz. All rights reserved.
```

### Naming

The following naming conventions should be followed:

* Interfaces: `I` prefix and `PascalCase` (e.g. `IInterface`)
* Non-Interface types (classes, structs, enums, delegates and namespaces): `PascalCase` (e.g. `ClassName`)
* Constant fields: `PascalCase` (e.g. `ConstantName`)
* Public, internal and protected static readonly fields: `PascalCase` (e.g. `StaticReadonlyFieldName`)
* Private static readonly fields: `_` prefix and `camelCase` (e.g. `_privateStaticReadonlyField`)
* Public symbols (properties, methods and events): `PascalCase` (e.g. `PublicSymbol`)
* Public, internal and protected readonly fields: `PascalCase` (e.g. `ReadonlyFieldName`)
* Protected fields: `camelCase` (e.g. `protectedField`)
* Private and private readonly fields: `_` prefix and `camelCase` (e.g. `_privateField`)
* Parameters: camelCase (e.g. `parameterName`)
* Local variables: camelCase (e.g. `localVariableName`)

### Spaces

Use spaces instead of tabs! (screw them tabs)

* Use 4 spaces for indentation
* Indent block content, switch labels, case contents and labels
* Use space after a comma (e.g. parameters)
* Use space after keywords in control flow statements
* Use space after a semicolon in `for` statements
* Use spaces before and after binary operators

But in most other cases, spaces are not required, especially the uncessary ones at the end of a line.

### Newlines

Newlines should be added in following cases:

* End of a file
* Before an opening brace (Allman style)
* Before the `else` keyword
* Before the `catch` keyword
* Before the `finally` keyword
* Before the members of an anonymous type (e.g. no `new { foo = "bar" }`)
* Before the members of an object initializer (e.g. no `new Foo { Bar = "baz" }`)
* Between query expression clauses (e.g. `from`, `where`, `select`)

### Braces

Generally saying, braces (e.g. `{ }`) are preferred for readability.

There are some exceptions to this rule, such as:

* Using statements, unless the explicit scope is required
* Single-line statements of simple nature (e.g. method with a simple return statement)

Additionally, braces should always be placed on a new line (Allman style) and the containing block must be properly indented. One exception to this rule are consecutive `using` statements, which can be placed on the same line without them having to be nested.

### Parentheses

Parentheses should be used in the following cases:

* Arithmetic binary operators (e.g. `a + (b * c)` over `a + b * c`)
* Relational binary operators (e.g. `(a < b) == (c > d)` over `a < b == c > d`)
* Other binary operators (e.g. `a || (b && c)` over `a || b && c`)

### Usings

Namespace imports should be placed after the file's namespace declaration.

The following ordering rules should be followed:

* `System.*` namespaces should be placed before other namespaces
* Namespaces should be sorted alphabetically

### Namespaces

Namespace declaration should be file-scoped and always match the folder structure of the file.

::: tip GOOD

```csharp
namespace Ayaka.Nuke;

public class Foo
{
}
```

:::

::: danger BAD

```csharp
namespace Ayaka.Nuke
{
    public class Foo
    {
    }
}
```

:::

### Modifiers

Modifiers should always be added, except for interface members.

The following ordering rules should be followed:

1. Access modifiers: `public` > `private` > `protected` > `internal` > `file`
2. Remaining modifiers: `static` > `new` > `abstract` > `virtual` > `sealed` > `readonly` > `override` > `extern` > `unsafe` > `volatile` > `required` > `async`

### Language keywords vs BCL types

Use language keywords like `int` over `Int32`, `string` over `String`, etc., regardless of whether it's for a local variable, method parameters, class members or when accessing static members of said types.

### 'this.' qualifier

Do not use `this.` to qualify member access unless it's necessary for disambiguation.

### 'var' usage

Use `var` as long as it is easy to guess its type either from the variable assignment or variable name and context.

### Variable declaration

The following rules should be followed when declaring variables:

* Declare one variable per line, unless you deconstruct a tuple or a similar construct
* Deconstruct tuples when possible
* Use the simple `default` keyword instead of `default(T)` when possible
* Use the implicit object creation (e.g. `Foo obj = new();`) when possible
* Inline variable declaration when applicable (e.g. `if (int.TryParse("123", out var result))`)

### Expressions

The following rules should be followed regarding expressions:

* Use `switch` expressions when possible
* Use expression bodies for single line methods and operators
* Use expression bodies for operators, properties, indexers, accessors and lambdas
* Do not use expression bodies for constructors and multi line methods or operators
* Use pattern matching when possible (e.g. `if (o is { Bar: > 0 })` over `if (o != null && o.Bar > 0)`)
* Use pattern matching with assignment over `is` with cast (e.g. `if (o is int i) { ... }` over `if (o is int) { int i = (int)o; ... }`)
* Use pattern matching over `as` with `null`check (e.g. `if (o is Foo foo)` over `if (o as Foo != null)`)
* Use the `not` keyword over `!` when pattern matching (e.g. `if (o is not Foo)` over `if (!(o is Foo))`)
* Use the extended property pattern when pattern matching nested properties (e.g. `if (o is Foo { Bar: { Baz: > 0 } })` over `if (o is Foo { Bar.Baz: > 0 })`)

### Null-checking

The following rules should be followed regarding null-checking:

* Use the null-conditional operator `?.` when possible
* Use coalescing operator `??` when possible (e.g. `var foo = bar ?? "default"` over `var foo = bar != null ? bar : "default"`)
* Use `is null` over `== null`

### Others

* Prefer primary constructor (that one is controversial, maybe I'll change my mind someday)
* Prefer index and range operators over `Substring`, `Take`, `Skip`, etc.
* Discard variables using `_` if not used (e.g. `var (id, firstName, _) = GetPerson();` whereas `GetPerson` returns a tuple `(int, string, string)`),

### Unnecessary code

Try to avoid unnecessary code! I hate useless clutter in my codebases.
