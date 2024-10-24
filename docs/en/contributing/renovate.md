# Renovate

Ayaka uses [renovate] to keep dependencies up-to-date.

## Configuration

The configuration for renovate is stored in the [.github/renovate.json] file.

It uses the following presets made by me

* [Default Preset] (transitive)
* [GitHub Actions Preset]
* [.NET Preset]

## Modifications

If you plan to make changes to the renovate configuration, please make sure you propose this in the correct place. In many cases, changes should probably be made in one of the presets instead of Ayaka's own configuration file.

[renovate]: https://www.mend.io/renovate/
[.github/renovate.json]: https://github.com/Xzelsius/Ayaka/blob/main/.github/renovate.json
[Default Preset]: https://github.com/Xzelsius/renovate-config/blob/main/default.json
[GitHub Actions Preset]: https://github.com/Xzelsius/renovate-config/blob/main/github-actions.json
[.NET Preset]: https://github.com/Xzelsius/renovate-config/blob/main/dotnet.json
