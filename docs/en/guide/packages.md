# Packages

Ayaka tries to split functionality in to small packages to keep dependencies to a minimum. This way you can use only the parts you need and keep your project small and lightweight.

Generally, we structure the packages around features, similar to the official Microsoft NuGet packages.

* All our packages are prefixed with `Ayaka`
* The remaining name parts represent the feature that the package extends or provides
* A package may end with `.Abstractions` which indicates a feature that may have different implementations but common abstractions

## Overview

The list of packages thate are currently part of Ayaka

| Package            | Description                                                                            | NuGet                                                                                             |
|--------------------|----------------------------------------------------------------------------------------|---------------------------------------------------------------------------------------------------|
| [Ayaka.Nuke](../nuke/index.md) | Provides various opinionated build components for simpler build automation using NUKE. | [![Ayaka.Nuke](https://img.shields.io/nuget/v/Ayaka.Nuke)](https://nuget.org/packages/Ayaka.Nuke) |
