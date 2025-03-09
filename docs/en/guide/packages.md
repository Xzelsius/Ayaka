# Packages

Ayaka tries to split functionality in to small packages to keep dependencies to a minimum. This way you can use only the parts you need and keep your project small and lightweight.

Generally, I structure the packages around features, similar to the official Microsoft NuGet packages.

* All packages are prefixed with `Ayaka`
* The remaining name parts represent the feature that the package extends or provides
* A package may end with `.Abstractions` which indicates a feature that may have different implementations but common abstractions

## Overview

The list of packages thate are currently part of Ayaka

| Package                           | Description                                                                            | NuGet                                                                                                                                                            |
|-----------------------------------|----------------------------------------------------------------------------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| [Ayaka.Nuke]                      | Provides various opinionated build components for simpler build automation using NUKE. | [![Ayaka.Nuke](https://img.shields.io/nuget/v/Ayaka.Nuke)](https://nuget.org/packages/Ayaka.Nuke)                                                                |
| [Ayaka.MultiTenancy.Abstractions] | Provides abstractions for multi-tenanted applications.                                 | [![Ayaka.MultiTenancy.Abstractions](https://img.shields.io/nuget/v/Ayaka.MultiTenancy.Abstractions)](https://nuget.org/packages/Ayaka.MultiTenancy.Abstractions) |
| [Ayaka.MultiTenancy]              | Provides functionality for creating multi-tenanted applications.                       | [![Ayaka.MultiTenancy](https://img.shields.io/nuget/v/Ayaka.MultiTenancy)](https://nuget.org/packages/Ayaka.MultiTenancy)                                        |
| [Ayaka.MultiTenancy.AspNetCore]   | Provides ASP.NET core specific extensions for multi-tenanted applications.             | [![Ayaka.MultiTenancy.AspNetCore](https://img.shields.io/nuget/v/Ayaka.MultiTenancy.AspNetCore)](https://nuget.org/packages/Ayaka.MultiTenancy.AspNetCore)       |

[Ayaka.Nuke]: ./packages/nuke/index
[Ayaka.MultiTenancy.Abstractions]: ./packages/multi-tenancy/index
[Ayaka.MultiTenancy]: ./packages/multi-tenancy/index
[Ayaka.MultiTenancy.AspNetCore]: ./packages/multi-tenancy/index
