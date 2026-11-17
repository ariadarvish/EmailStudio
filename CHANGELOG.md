# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.1.0] - 2026-11-17

### Added
- **Core Templating Engine (`EmailStudio`)**:
  - Pure model architecture with immutable `EmailDraft` (Subject, Html, PlainText).
  - Frontmatter parsing with YAML metadata support (`subject`, `preheader`, `tags`).
  - Six built-in email components: `<Button>`, `<Heading>`, `<Divider>`, `<Spacer>`, `<Image>`, `<T.Try>`.
  - Nested component expansion and table-safe HTML generation.
  - Template evaluation supporting dynamic expressions (`@Model.Property`), `@if/@else`, `@foreach`, `<Partial>`, and `<Include>`.
  - Rejection of runtime Razor execution vulnerabilities (`@functions`, `@inject`, `@await`).
  - PreMailer.Net CSS inlining with responsive `@media` query preservation in `<head>`.
  - Clean markdown-like plain text generation formatting links as `Text (URL)`.
  - Email size guard with warning at 90 KB and error at 102 KB (Gmail clipping prevention).
  - Multi-target support for `.NET 10` (`net10.0`) and `.NET 11` (`net11.0`).
- **Roslyn Incremental Source Generator (`EmailStudio.Generator`)**:
  - `IIncrementalGenerator` targeting `netstandard2.0` for zero-runtime Razor compilation.
  - Compile-time diagnostics `TPL001` through `TPL009` with actionable error messages.
  - Strongly-typed `EmailTemplates.Render<Name>(model, culture)` method emission.
  - MSBuild props auto-registration (`build/EmailStudio.props`) for `Templates/**/*.cshtml`.
- **Hot-Reload Preview Server (`EmailStudio.Preview`)**:
  - ASP.NET Core minimal API host running on Kestrel.
  - Embedded zero-npm developer dashboard with Desktop (700px) and Mobile (375px) viewports.
  - Dark mode simulation toggle and culture switcher.
  - Live syntax error overlay displaying diagnostic code, error message, file, and line number.
  - 200 ms debounced file watching with WebSocket instant hot reload broadcast.
  - Test SMTP forwarding to local catchers (e.g. Mailpit/Inbucket on port 1025).
- **Command-Line Interface (`EmailStudio.Tool`)**:
  - Global .NET CLI tool (`dotnet email-preview` / `dotnet-email-preview`).
  - `init [path]`: Scaffolds a starter template project with layout, partials, and samples.
  - `serve [path] [--port <p>]`: Launches the hot-reload preview server.
  - `render [path] [--out <dir>] [--culture <c>]`: Statically compiles templates to disk.
- **Sample Gallery**:
  - 10 production-ready transactional templates in `samples/Gallery`: `Welcome`, `ResetPassword`, `InvoicePaid`, `InvoiceOverdue`, `TeamInvite`, `PasswordChanged`, `WeeklyDigest`, `ShippingNotification`, `AccountDeleted`, `TwoFactorEnabled`.
  - Sample datasets (`*.sample.json`) and i18n dictionaries (`de-DE`, `fr-FR`).
- **Packaging & CI/CD**:
  - Packaged `EmailStudio` NuGet library embedding source generator analyzer and build props.
  - Packaged `EmailStudio.Tool` as a global dotnet tool.
  - GitHub Actions CI workflow for cross-platform build, test, and package verification.

