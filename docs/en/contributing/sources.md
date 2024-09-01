# Sources

Ayaka sources are available on [GitHub](https://github.com/Xzelsius/Ayaka) under the [MIT license](https://github.com/Xzelsius/Ayaka?tab=MIT-1-ov-file#readme).

## Repository structure

The repository is structured as follows:

```text
<repository-root>/
├── .github/                    # Everything GitHub related
│   └── workflows               # GitHub Actions workflows
│   └── ...
├── .nuke/                      # The NUKE build configuration used for the CLI
│   └── ...
├── nukebuild/                  # The actual NUKE build project
│   └── ...
├── docs/                       # The VitePress based docs site
│   ├── .vitepress              # VitePress configuration files
│   ├── en/                     # English documentation
│   │   └── ...
│   └── ...
├── build/                      # MSBuild files required for the packages
│   ├── assets/                 # Assets used in the build process (e.g. package logo)
│   │   └── ...
│   ├── *.props
│   └── *.target
├── src/                        # The actual source code of Ayaka packages
│   ├── <package-directory>     # Each package has its own directory (e.g. Ayaka.Nuke)
│   └── ...
└── test/
    └── <package-directory>     # The test projects for each package
    └── ...                     # Each package has its own directory (e.g. Ayaka.Nuke.Tests)
```

If you have any questions or need help to find something, feel free to ask at [GitHub discussions](https://github.com/Xzelsius/Ayaka/discussions).
