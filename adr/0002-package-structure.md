# 2. Package Architecture and Target Framework Selection

Date: 2026-11-17

## Status

Accepted

## Context

We need an extensible packaging structure that enables:
1. Seamless library consumption in backend applications.
2. Roslyn analyzer integration without leaking Roslyn assembly dependencies to end users.
3. Standalone local previewing via a .NET CLI tool (`dotnet email-preview`).
4. Modern runtime support (.NET 10.0+), dropping deprecated out-of-support runtimes (.NET 8, .NET 9).

## Decision

1. **Package Separation**:
   - `EmailStudio`: Primary NuGet package containing public runtime APIs (`EmailDraft`, `IEmailRenderer`, DI extensions). Packages `EmailStudio.Generator` internally as an analyzer asset (`analyzers/dotnet/cs`).
   - `EmailStudio.Generator`: Targets `netstandard2.0` (required for Roslyn incremental source generator compatibility with MSBuild and Visual Studio).
   - `EmailStudio.Preview`: ASP.NET Core minimal API host library for browser-based previewing and hot reloading.
   - `EmailStudio.Tool`: Global .NET CLI tool (`dotnet-email-preview`) embedding the preview host and CLI commands (`init`, `serve`, `render`).
2. **Target Framework Strategy**:
   - Target framework is `net10.0` (with forward-compatibility for `net11.0`).
   - End-of-life frameworks (`net8.0`, `net9.0`) are not targeted.

## Consequences

- Clean separation between compilation-time tooling, runtime libraries, and CLI developer tooling.
- Roslyn binaries remain private analyzer dependencies and do not collide with consumer application dependencies.

