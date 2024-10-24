# .NET Build Tasks

The .NET builds tasks are methods that are meant be executed as part of a target.

## DotNetValidateTasks

The `DotNetValidateTasks` provides tasks to validate .NET NuGet packages using the `dotnet-validate` tool.

By default, the `DotNetValidateTasks` use the default logger and exit handler from the `Nuke.Common` NuGet package.
If they don't suit your needs, you can override them by setting the `DotNetValidateLogger` or
`DotNetValidateExitHandler` properties to something different. It's also possible to override them for an individual task
execution by passing them as arguments to the task method.

::: warning
You may need to install the `dotnet-validate` CLI tool before using this target.

The simplest way to do this is by adding `<PackageDownload Include="dotnet-validate" Version="..." />` to your
build project file.

See [Executing CLI Tools] for more information.
:::

### Validate Local Package

Using `DotNetValidateLocalPackage()` you can validate a local NuGet package.

This method takes a `DotNetValidateRemotePackageSettings` object as an argument. The settings object has the following
properties which can be set:

| Parameter      | Required | Description                                     |
|----------------|----------|-------------------------------------------------|
| `PackagePath`  | `true`   | The path to the local NuGet package to validate |

::: code-group

```csharp [Usage]
DotNetValidateTasks.DotNetValidateLocalPackage(
    new DotNetValidateLocalPackageSettings()
        .SetPackagePath("path/to/package.nupkg"));
```

:::

### Validate Remote Package

Using `DotNetValidateRemotePackage()` you can validate a remote NuGet package.

This method takes a `DotNetValidateRemotePackageSettings` object as an argument. The settings object has the following
properties which can be set:

| Parameter         | Required | Description                                                                                                                     |
|-------------------|----------|---------------------------------------------------------------------------------------------------------------------------------|
| `PackageId`       | `true`   | The ID of the NuGet package to validate                                                                                         |
| `PackageVersion`  | `false`  | The optional version of the NuGet package to validate. Defaults to the latest package version if not specified                  |
| `ConfigDirectory` | `false`  | The directory where to look for a NuGet configuration to use for validation. Defaults to the current directory if not specified |

::: code-group

```csharp [Usage]
DotNetValidateTasks.DotNetValidateRemotePackage(
    new DotNetValidateRemotePackageSettings()
        .SetPackageId("PackageId")
        .SetPackageVersion("1.2.3"));
```

[Executing CLI Tools]: https://nuke.build/docs/common/cli-tools
