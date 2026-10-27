# Sources

Ayaka sources are available on [GitHub] under the [MIT license].

## Repository structure

The repository is structured as follows:

```text
<repository-root>/
├── .config/                    # Local .NET tools used by the build (e.g. GitVersion)
├── .github/                    # Everything GitHub related
│   ├── workflows/              # GitHub Actions workflows
│   └── ...
├── docs/                       # The VitePress based docs site
│   ├── .vitepress/             # VitePress configuration files
│   ├── en/                     # English documentation
│   ├── public/                 # Static assets (e.g. logos)
│   └── ...
├── eng/                        # Shared MSBuild files imported by all projects
│   ├── assets/                 # Assets used in the build process (e.g. package logo)
│   └── *.props
├── src/                        # The actual source code of Ayaka packages
│   ├── <package-directory>/    # Each package has its own directory (e.g. Ayaka.MultiTenancy)
│   └── Directory.Build.props
├── test/                       # The test projects
│   ├── <package-directory>/    # Each tested package has its own directory (e.g. Ayaka.MultiTenancy.Tests)
│   └── Directory.Build.props
├── Ayaka.slnx                  # The solution file
├── build.sh                    # The build entry point, used locally and by CI
├── GitVersion.yml              # Versioning configuration
└── global.json                 # The pinned .NET SDK version
```

If you have any questions or need help to find something, feel free to ask at [GitHub discussions].

[GitHub]: https://github.com/Xzelsius/Ayaka
[MIT license]: https://github.com/Xzelsius/Ayaka?tab=MIT-1-ov-file#readme
[GitHub discussions]: https://github.com/Xzelsius/Ayaka/discussions
