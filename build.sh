#!/usr/bin/env bash
#
# Builds, tests and packs Ayaka. CI runs the same steps, use Git Bash on Windows.
#
#   ./build.sh                 # build test pack
#   ./build.sh build test      # only the given steps, in the given order
#   ./build.sh docs
#
# Steps: version, build, test, pack, validate (passes on CI builds only), docs

set -euo pipefail
cd "$(dirname "${BASH_SOURCE[0]}")"

export DOTNET_CLI_TELEMETRY_OPTOUT=1
export DOTNET_NOLOGO=1

artifacts="artifacts"
no_build=()

restore_tools() {
  [[ -z ${tools_restored:-} ]] || return 0
  dotnet tool restore
  tools_restored=1
}

# Reads the GitVersion_* variables line by line, values are never evaluated by the shell
step_version() {
  [[ -z ${GitVersion_SemVer:-} ]] || return 0
  restore_tools

  local output line
  output=$(dotnet gitversion -output dotenv -nofetch -nocache)
  while IFS= read -r line; do
    line=${line%$'\r'}
    if [[ $line =~ ^(GitVersion_[A-Za-z]+)=\'(.*)\'$ ]]; then
      declare -g "${BASH_REMATCH[1]}=${BASH_REMATCH[2]}"
    fi
  done <<< "$output"

  echo "Version: $GitVersion_SemVer"
}

step_build() {
  step_version
  dotnet build Ayaka.slnx -c Release \
    -p:AssemblyVersion="$GitVersion_AssemblySemVer" \
    -p:FileVersion="$GitVersion_AssemblySemFileVer" \
    -p:InformationalVersion="$GitVersion_InformationalVersion"
  no_build=(--no-build)
}

step_test() {
  rm -rf "$artifacts/test-results"
  dotnet test Ayaka.slnx -c Release "${no_build[@]}" \
    --results-directory "$artifacts/test-results" \
    --collect "XPlat Code Coverage" \
    ${GITHUB_ACTIONS:+--logger GitHubActions}
}

step_pack() {
  step_version

  # With a pre-release label (main, PRs) Version overrides the projects' VersionSuffix, so all packages
  # share one version. Without one (tag builds) only the prefix is set and preview packages keep their suffix.
  local version="-p:VersionPrefix=$GitVersion_SemVer"
  if [[ -n $GitVersion_PreReleaseLabel ]]; then
    version="-p:Version=$GitVersion_SemVer"
  fi

  rm -rf "$artifacts/packages"
  dotnet pack Ayaka.slnx -c Release "${no_build[@]}" --output "$artifacts/packages" "$version"
}

step_validate() {
  restore_tools

  local package failed=0
  for package in "$artifacts"/packages/*.nupkg; do
    dotnet validate package local "$package" || failed=1
  done
  return "$failed"
}

step_docs() (
  cd docs
  npm ci
  npm run lint:js
  npm run build -- --outDir "../$artifacts/docs"
)

if (($# == 0)); then
  set -- build test pack
fi

for step in "$@"; do
  case $step in
    version | build | test | pack | validate | docs) "step_$step" ;;
    *)
      echo "Unknown step '$step'. Available: version build test pack validate docs" >&2
      exit 1
      ;;
  esac
done
