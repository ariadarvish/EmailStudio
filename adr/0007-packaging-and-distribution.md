# ADR 0007: Packaging, Analyzer Distribution, and CLI Tooling

## Status
Accepted

## Date
2026-11-17

## Context
EmailStudio delivers both a developer authoring engine and a compile-time source generator, as well as a standalone CLI preview tool. In the .NET ecosystem, distributing Roslyn incremental generators requires careful layout in NuGet packages:
1. Roslyn generators must target `netstandard2.0` and be placed in `analyzers/dotnet/cs/` inside the `.nupkg`.
2. The runtime library target frameworks (`net10.0;net11.0`) must reside in `lib/net10.0/` and `lib/net11.0/`.
3. Consumers should not need to reference both `EmailStudio` and `EmailStudio.Generator` separately; referencing `EmailStudio` must transparently light up both the runtime engine (`EmailDraft`, `IEmailRenderer`) and compile-time code generation.
4. The CLI tool must be installable via `dotnet tool install -g EmailStudio.Tool` and invoke the command `dotnet-email-preview`.

## Decision
1. **Single Library Package (`EmailStudio`)**:
   - `EmailStudio.Generator` has `<IsPackable>false</IsPackable>` and is built targeting `netstandard2.0`.
   - `EmailStudio.csproj` packages `EmailStudio.Generator.dll` directly into `analyzers/dotnet/cs`.
   - `EmailStudio.props` is packaged into `build/EmailStudio.props`, ensuring any consuming project automatically registers `Templates/**/*.cshtml` and `Locales/**/*.json` as MSBuild `AdditionalFiles`.
2. **Dedicated Tool Package (`EmailStudio.Tool`)**:
   - `EmailStudio.Tool.csproj` is marked with `<PackAsTool>true</PackAsTool>` and `<ToolCommandName>dotnet-email-preview</ToolCommandName>`.
   - Targets `net11.0`, packing self-contained tool binaries into `tools/net11.0/any/`.
3. **Zero-Warning Packaging**:
   - `EmailStudio.Preview` is a library project marked with `<IsPackable>false</IsPackable>` and `<WarnOnPackingNonPackableProject>false</WarnOnPackingNonPackableProject>` to eliminate build warnings when running solution-wide `dotnet pack`.
   - All test projects are marked with `<IsPackable>false</IsPackable>`.

## Consequences
- **Positive**: Consumers simply run `dotnet add package EmailStudio`. They immediately get typed compile-time render methods generated and runtime classes without complex configuration.
- **Positive**: The preview CLI is globally installable with one command and works seamlessly across projects.
- **Trade-off**: The generator DLL must remain strictly `netstandard2.0` with no external dependencies that aren't packaged or bundled with Roslyn.

