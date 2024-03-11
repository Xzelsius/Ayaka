# Code Guidelines

The general rule we follow is **use Visual Studio defaults**.

1. We use Allman style braces, where each brace begins on a new line. A single line statement block
   can go without braces but the block must be properly indented on its own line and it must not be
   nested in other statement blocks that use braces (See rule 15 for more details). One exception
   is that a using statement is permitted to be nested within another using statement by starting
   on the following line at the same indentation level, even if the nested using contains a
   controlled block.
2. We use four spaces of indentation (no tabs).
3. We use `_camelCase` for internal and private fields and use `readonly` where possible. Prefix
   internal and private instance fields with `_`, static fields with `s_` and thread static fields
   with `t_`. When used on static fields, `readonly` should come after static (e.g.
   `static readonly` not `readonly static`). Public fields should be used sparingly, but when they
   are used, they should use `PascalCasing`, no prefix.
4. We avoid `this.` unless absolutely necessary.
5. We always specify the visibility, even if it's the default (e.g. `private string _foo` not
   `string _foo`). Visibility should be the first modifier (e.g. `public abstract` not
   `abstract public`).
6. Namespace imports should be specified at the top of the file, _outside_ of `namespace`
   declarations and should be sorted alphabetically, with the exception of `System.*` namespaces,
   which are to be placed on top of all others.
7. Avoid more than one empty line at any time. For example, do not have two blank lines between
   members of a type.
8. Avoid spurious free spaces. For example avoid `if (someVar == 0)...`, where the dots mark the
   spurious free spaces.
9. We only use `var` when it's obvious what the variable type is (e.g. `var stream = new FileStream(...)`
   not `var stream = OpenStandardInput()`).
10. We use language keywords instead of BCL types (e.g. `int`, `string`, `float` instead of
   `Int32`, `String`, `Single`, etc) for both type references as well as method calls (e.g. `int.Parse`
   instead of `Int32.Parse`)
11. We use `PascalCasing` to name all our constant local variables and fields. The only exception
   is for interop code where the constant value should exactly match the name and value of the
   code you are calling via interop.
12. We use `nameof(...)` instead of `"..."` whenever possible and relevant.
13. Fields should be specified at the top within type declarations.
14. When including non-ASCII characters in the source code use Unicode escape sequences (`\uXXXX`)
   instead of literal characters. Literal non-ASCII characters occasionally get garbled by a tool or
   editor.
15. When using a single-statement if, we follow these conventions:
    * Never use single-line form (for example: `if (source == null) throw new ArgumentNullException("source");`)
    * Using braces is always accepted, and required if any block of an `if/else` / `if/.../else` compound
      statement uses braces or if a single statement body spans multiple lines.
    * Braces may be omitted only if the body of every block associated with an `if/else` / `if/.../else`
      compound statement is placed on a single line.
