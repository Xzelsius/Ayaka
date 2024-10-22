# .NET Build Components

The general build components are suitable for .NET based applications.

## Build Context

The build context components extend the build with additional information required during the build.

### IHaveDotNetConfiguration

The `IHaveDotNetConfiguration` build component decorates the build with the .NET configuration to be
used during the build.

| Property              | Description                                        |
|-----------------------|----------------------------------------------------|
| `DotNetConfiguration` | The .NET configuration to be used during the build |

Ayaka knows two `Configuration` values:

* `Debug`: Which is the default if `IsLocalBuild` is `true`
* `Release`: Which is the default if `IsLocalBuild` is `false`

::: code-group

```csharp {3,9} [Usage]
class Build
    : NukeBuild,
        IHaveDotNetConfiguration
{
    public static int Main() => Execute<Build>(x => x.Default);

    Target Default => _ => _
        .Executes(() => {
            Log.Information($"Configuration: {((IHaveDotNetConfiguration)this).DotNetConfiguration}");
        });
}
```

```csharp {7} [Change the value in Code]
class Build
    : NukeBuild,
        IHaveDotNetConfiguration
{
    public static int Main() => Execute<Build>(x => x.Default);

    Configuration IHaveDotNetConfiguration.DotNetConfiguration => Configuration.Debug;

    Target Default => _ => _
        .Executes(() => {
            Log.Information($"Configuration: {((IHaveDotNetConfiguration)this).DotNetConfiguration}");
        });
}
```

```bash {2} [Change the value with Parameter]
dotnet nuke \
   --dotnet-configuration "Debug"
```

```bash {1} [Change the value with Environment Variable]
export DOTNET_CONFIGURATION="Debug"
dotnet nuke
```

:::

### IHaveNuGetConfiguration

## Build Targets

### ICanDotNetRestore / IHaveDotNetRestoreTarget

### ICanDotNetBuild / IHaveDotNetBuildTarget

### ICanDotNetTest / IHaveDotNetTestTarget

### ICanDotNetPack / IHaveDotNetPackTarget

### ICanDotNetPush / IHaveDotNetPushTarget

### ICanDotNetValidate / IHaveDotNetValidateTarget
