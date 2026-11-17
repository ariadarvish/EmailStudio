# 4. Incremental Source Generator Architecture & Compile-Time Diagnostics

Date: 2026-11-17

## Status

Accepted

## Context

To provide a "react-email for .NET" experience with zero runtime Razor compilation and first-class Native AOT compatibility, email templates must be transformed into typed C# render methods at compile time. In addition, developers need immediate feedback in the IDE and during `dotnet build` if a template property is misspelled, a layout is missing, or an email exceeds the Gmail size limit.

## Decision

1. **Incremental Generator Model**:
   - `EmailStudioGenerator` implements Roslyn's `IIncrementalGenerator` targeting `netstandard2.0`.
   - Scans `AdditionalFiles` (`Templates/**/*.cshtml`, `Templates/**/*.sample.json`, and `Locales/**/*.json`).
   - Pipeline uses immutable value providers with structural equality to ensure cache hits on unaffected files.

2. **Diagnostics (TPL001–TPL009)**:
   - Emits Roslyn diagnostics with exact line/span mappings into the source `.cshtml` files:
     - `TPL001`: Template parsing failure (syntax/front matter).
     - `TPL002`: Property reference validation against `@model` semantic symbols in compilation.
     - `TPL003`: Referenced layout or partial missing from compilation items.
     - `TPL004`: Sample JSON validation (missing or malformed JSON).
     - `TPL005`: Missing `subject` in template front matter.
     - `TPL006`: Duplicate template identifiers.
     - `TPL007`: Rendered size threshold exceeded (warning > 90 KB, error > 102 KB).
     - `TPL008`: Unsupported Razor syntax (`@functions`, `@await`, `@section`, etc.).
     - `TPL009`: Missing localization key in dictionary under strict mode.

3. **Deterministic Output & Native AOT**:
   - Generates static partial class `EmailTemplates` with strongly typed `Render<TemplateName>(TModel model, string? culture = null)` methods.
   - Code emission is sorted alphabetically by template name with invariant culture formatting, without timestamps or nondeterministic hashes.
   - Execution paths are reflection-free and trim-safe.

## Consequences

- Full IDE autocomplete for `EmailTemplates.RenderWelcome(...)`.
- Instant compile-time diagnostic feedback without launching the application.
- Sub-second incremental compilation overhead in Roslyn.

