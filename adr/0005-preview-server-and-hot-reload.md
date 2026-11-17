# 5. Local Preview Server, Viewports, and Hot-Reload Architecture

Date: 2026-11-17

## Status

Accepted

## Context

A core goal of EmailStudio is providing a sub-second "edit-save-see" local feedback loop for .NET developers. In contrast to existing workflows where developers send test emails or manually refresh browsers, the local preview server (`dotnet email-preview serve`) runs locally, watches template directories, re-renders templates instantly, and pushes updates via WebSockets without page reloads.

## Decision

1. **Host Architecture**:
   - Built on ASP.NET Core Minimal APIs hosted via Kestrel on `http://localhost:5173`.
   - Native WebSocket middleware mounted at `/ws` for bi-directional live reload signaling.
   - Embedded static frontend (zero npm, zero bundler, zero external CDN dependencies) functioning completely offline.

2. **File System Watching & Debouncing**:
   - `System.IO.FileSystemWatcher` monitors `Templates/` and `Locales/`.
   - Ignores editor temporary files (`*.tmp`, `~*`) and hidden directories.
   - 200 ms sliding-window debouncing prevents redundant re-renders on multi-file saves.

3. **Developer Interface**:
   - **Viewports**: Desktop (700px) and Mobile (375px) side-by-side or toggleable.
   - **Dark Mode Simulation**: Toggles email background container and `color-scheme: dark` / `prefers-color-scheme` CSS contexts.
   - **Multi-Tab Inspection**: `Preview` (live rendered iframe), `HTML` (syntax-highlighted raw HTML), `Text` (plain-text alternative), and `Diagnostics` (compiler warnings/errors).
   - **Send to Inbox**: Integrates with local SMTP catchers (defaulting to Mailpit at `localhost:1025`) via direct SMTP socket dispatch.

## Consequences

- Zero build steps or Node.js dependencies for developer preview.
- Extremely low memory footprint (< 100 MB).
- Real-time updates in under 200 ms from disk save.

