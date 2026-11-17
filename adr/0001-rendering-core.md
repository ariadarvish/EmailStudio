# 1. Rendering Core and Zero-Runtime-Razor Architecture

Date: 2026-11-17

## Status

Accepted

## Context

Transactional email in .NET has historically relied on runtime Razor compilation (e.g. RazorLight, custom Razor view engines), which introduces substantial cold-start latency, large runtime dependencies, complex shadow-copying of assemblies, and incompatibility with Native AOT publishing.

Alternative approaches include building on source-generated Razor models (such as Damian Edwards' RazorSlices) or building a focused syntax-subset compiler for email templates.

## Decision

EmailStudio adopts compile-time Roslyn source generation:
1. Templates (`.cshtml` files) are parsed and compiled into typed C# rendering methods at build time.
2. Runtime Razor compilation is completely omitted (zero runtime Razor dependency).
3. The rendered pipeline generates an immutable `EmailDraft(Subject, HtmlBody, TextBody)` with CSS inlining and plain text fallback.
4. Native AOT compilation is fully supported for all rendering paths.

## Consequences

### Positive
- Blazing-fast rendering performance (< 5ms per template).
- Zero cold-start compilation overhead at runtime.
- Full compile-time safety and IDE diagnostics (`TPL001`–`TPL009`).
- Native AOT compatibility.

### Negative
- Dynamic runtime template loading without recompiling the application is not supported by design.

