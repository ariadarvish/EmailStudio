# 6. CLI Command Architecture and Production Template Gallery

Date: 2026-11-17

## Status

Accepted

## Context

Developers need an effortless onboarding flow (`init → serve → edit`) and automated headless rendering (`render`) for CI pipelines without requiring custom scaffolding scripts. Furthermore, production-grade email development requires a battle-tested gallery of standard transactional email templates serving both as reference fixtures and living documentation.

## Decision

1. **CLI Commands (`dotnet email-preview`)**:
   - `init [path]`: Scaffolds a complete email project with `_Layouts/Base.cshtml`, starter templates (`Welcome`, `ResetPassword`, `InvoicePaid`), sample JSON files, and locale dictionaries (`de-DE.json`, `fr-FR.json`).
   - `serve [path] [--port 5173] [--smtp host:port]`: Boots Kestrel preview server with debounced file watching and WebSocket hot reload.
   - `render [path] [--culture <lang>] [--out <dir>]`: Headless CI command batch-rendering all templates, enforcing the 102 KB Gmail size guard, and emitting exit code 0 (success) or 1 (failure).

2. **Production Gallery (10 Templates)**:
   - Auth & Security: `Welcome`, `ResetPassword`, `PasswordChanged`, `TwoFactorEnabled`.
   - Billing & Commerce: `InvoicePaid`, `InvoiceOverdue`, `ShippingNotification`.
   - Collaboration & Lifecycle: `TeamInvite`, `WeeklyDigest`, `AccountDeleted`.
   - Each template includes realistic `.sample.json` test fixtures, table-safe components, and localization keys.

## Consequences

- Full parity with modern email tooling loops like `react-email`.
- Seamless CI/CD integration using `dotnet email-preview render --out dist`.

