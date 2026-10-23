# Contributing to Alba

Thanks for your interest in contributing!

## Building and testing

Alba builds with the .NET SDK pinned in `global.json` and uses a [Nuke](https://nuke.build) build:

```bash
# Compile everything (Release)
./build.cmd            # or ./build.sh on Linux/macOS

# Run the full test suite (xUnit + NUnit + TUnit sample projects)
./build.cmd Test
```

`global.json` puts `dotnet test` in Microsoft.Testing.Platform mode, so name a single test project
with `--project`, for example `dotnet test --project src/Alba.Testing`.

Or work directly with the solution at `src/Alba.sln` from your IDE or `dotnet` CLI. The main test
project is `src/Alba.Testing`.

`src/AspireSamples` holds the sample for the Aspire documentation page. Its tests start Postgres and
RabbitMQ containers, so they need Docker and are not part of `./build.cmd Test`. Run them with
`dotnet test --project src/AspireSamples`.

Package versions are managed centrally in `src/Directory.Packages.props`.

## Documentation

The docs are a VitePress site under `docs/`, with code samples embedded from the test projects via
[MarkdownSnippets](https://github.com/SimonCropp/MarkdownSnippets) `#region sample_*` markers. To run
the docs locally:

```bash
npm install
npm run docs
```

If you change a `sample_*` region in code, the corresponding docs page updates when the snippets
tool runs.

## Pull requests

- Open an issue first for anything beyond a small fix so the approach can be discussed.
- Add or update tests for behavior changes — `src/Alba.Testing` has acceptance-style coverage for
  most features.
- Breaking changes should include an entry in the current major-version changelog file (e.g.
  `v9_CHANGELOG.md`) with a short migration snippet.
- Target the `master` branch.

## Getting help

Questions are welcome on the [JasperFx Discord](https://discord.gg/WMxrvegf8H) or via GitHub
discussions/issues.
