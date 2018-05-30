# Contributing to Ayaka

Looking to contribute something to Ayaka? Here are some basic rules we would like you to follow

- [Code of Conduct](#code-of-conduct)
- [Found a Bug?](#bug-reports)
- [Missing a Feature?](#feature-requests)
- [Submission Guidelines](#pull-requests)
- [Coding Guidlines](#code-guidelines)

## Code of Conduct

Please read and follow our [Code of Conduct][coc].

## Bug reports

A bug is a demonstrable problem that is caused by the code in the repository. Good bug reports are extremely helpful, so thanks!

Guidelines for bug reports:

1. Check if the issue has already been reported — [Issues][issues]

2. Check if the issue has been fixed — Try to reproduce it using the latest `master` or `dev` branch

3. Isolate the problem

4. Submit using the `Bug report` template — [Submit a Bug report][submit-bug]

A good bug report shouldn't leave other needing to chase you up for more information. Please try to be as detailed as possible in your report.

## Feature requests

Feature requests are welcome. But take a moment to find out whether your idea fits with the scope and aims of the project. It's up to you to make a strong case to convince the project's developers of the merits of this feature.

Guidlines for feature requests:

1. Check if the feature has already been requested — [Issues][issues]

2. Submit using the `Feature request` template — [Submit a feature request][submit-feature]

Please provide as much detail and context as possible.

## Pull requests

Good pull requests are a fantastic help. They should remain focused in scope and avoid containing unrelated commits.

Guidlines for pull requests:

1. Fork the project and clone your fork

2. If you cloned a while ago, get the latest changes from upstream

3. Create a new topic branch to contain your feature, change, or fix

4. Commit your changes in logical chunks

5. Locally merge (or rebase) the upstream development branch into your topic branch

6. Push your topic branch up to your fork

7. Open a Pull request against the `dev` branch — [Open a Pull Request][open-pull-request]

See `twbs/bootstrap` pull request guidlines for more information — [link][pull-request-guidelines]

## Code guidelines

The general rule we follow is **use Visual Studio defaults**.

1. We use Allman style braces, where each brace begins on a new line. A single line statement block can go without braces but the block must be properly indented on its own line and it must not be nested in other statement blocks that use braces

2. We use four spaces of indentation (no tabs).

3. We use `_camelCase` for internal and private fields and use `readonly` where possible. Prefix internal and private instance fields with `_`, static fields with `s_` and thread static fields with `t_`. When used on static fields, `readonly` should come after static (e.g. `static readonly` not `readonly static`). Public fields should be used sparingly, but when they are used, they should use `PascalCasing`, no prefix.

4. We avoid `this.` unless absolutely necessary.

5. We always specify the visibility, even if it's the default (e.g. `private string _foo` not `string _foo`). Visibility should be the first modifier (e.g. `public abstract` not `abstract public`).

6. Namespace imports should be specified at the top of the file, _outside_ of `namespace` declarations and should be sorted alphabetically.

7. Avoid more than one empty line at any time. For example, do not have two blank lines between members of a type.

8. Avoid spurious free spaces. For example avoid `if (someVar == 0)...`, where the dots mark the spurious free spaces.

9. We only use `var` when it's obvious what the variable type is (e.g. `var stream = new FileStream(...)` not `var stream = OpenStandardInput()`).

10. We use language keywords instead of BCL types (e.g. `int`, `string`, `float` instead of `Int32`, `String`, `Single`, etc) for both type references as well as method calls (e.g. `int.Parse` instead of `Int32.Parse`)

11. We use `PascalCasing` to name all our constant local variables and fields. The only exception is for interop code where the constant value should exactly match the name and value of the code you are calling via interop.

12. We use `nameof(...)` instead of `"..."` whenever possible and relevant.

See `dotnet/coreclr` code guidlines for more information — [link][code-guidelines]

## Attribution

Inspired by `angular/angular`, `twbs/bootstrap` and `dotnet/coreclr`

[coc]: https://github.com/Xzelsius/Ayaka/blob/master/CODE_OF_CONDUCT.md
[issues]: https://github.com/Xzelsius/Ayaka/issues
[submit-bug]: https://github.com/Xzelsius/Ayaka/issues/new?template=BUG_REPORT.md
[submit-feature]: https://github.com/Xzelsius/Ayaka/issues/new?template=FEATURE_REPORT.md
[pulls]: https://github.com/Xzelsius/Ayaka/pulls
[open-pull-request]: https://help.github.com/articles/about-pull-requests/
[pull-request-guidelines]: https://github.com/twbs/bootstrap/blob/v4-dev/.github/CONTRIBUTING.md#pull-requests
[code-guidelines]: https://github.com/dotnet/corefx/blob/master/Documentation/coding-guidelines/coding-style.md
