# Good First Issues & Contributor Backlog

Pre-written backlog issues ready for GitHub triage, labeled `good first issue` and `help wanted` to encourage community onboarding.

---

## Issue 1: Add `<BulletList>` and `<ListItem>` components with bullet character customization

**Labels**: `good first issue`, `enhancement`  
**Milestone**: `v0.2.0`

### Description
Currently, authors create lists using standard HTML `<ul>` and `<li>` elements, which render inconsistently across Outlook (which adds excessive margins) and mobile clients. We want to add native `<BulletList>` and `<ListItem>` components that expand into table-safe HTML.

### Requirements
- Component syntax:
  ```html
  <BulletList bullet="•" spacing="8px">
    <ListItem>First item</ListItem>
    <ListItem>Second item</ListItem>
  </BulletList>
  ```
- Expands to Outlook-safe table cells with consistent bullet alignment.
- Supports custom bullet characters (`•`, `-`, `✓`, `→`) or numbers (`1.`, `2.`).
- Emits clean markdown bullets in plain text generation.
- Add unit tests in `ComponentExpanderTests.cs` and plain text tests in `PlainTextGeneratorTests.cs`.

---

## Issue 2: Add `<SocialLinks>` component for Twitter/X, GitHub, LinkedIn, YouTube, Discord

**Labels**: `good first issue`, `enhancement`  
**Milestone**: `v0.2.0`

### Description
Transactional emails commonly feature social media follow/channel links in their footers. Authoring table cells with inline SVGs or images repeatedly is error-prone. We want a `<SocialLinks>` component.

### Requirements
- Component syntax:
  ```html
  <SocialLinks iconSize="24px" spacing="12px">
    <Link platform="github" href="https://github.com/emailstudio" />
    <Link platform="x" href="https://x.com/emailstudio" />
    <Link platform="linkedin" href="https://linkedin.com/company/emailstudio" />
  </SocialLinks>
  ```
- Built-in SVG/PNG icons for popular platforms.
- Responsive centering and mobile touch targets (minimum 44x44px hit areas).
- Add unit tests in `ComponentExpanderTests.cs`.

---

## Issue 3: Port PreMailer.Net call to an in-process, zero-dependency CSS parser for 3x render speedup

**Labels**: `help wanted`, `performance`  
**Milestone**: `v0.2.0`

### Description
Currently, `CssInliner` utilizes `PreMailer.Net` (which internally depends on `AngleSharp`). While functionally complete, profiling indicates that DOM parsing and selector matching account for ~70% of total render latency.

### Requirements
- Benchmark current `CssInliner.InlineCss` throughput using `BenchmarkDotNet`.
- Explore a lightweight, specialized CSS selector tree parser tailored for email subset styles (classes, tags, IDs).
- Target a 2x-3x speedup with zero additional package dependencies.
- Ensure 100% parity with existing snapshot tests and responsive `@media` preservation.

---

## Issue 4: Add VS Code extension integrating the preview server via webview panel

**Labels**: `help wanted`, `tooling`  
**Milestone**: `v0.3.0`

### Description
Developers authoring `.cshtml` templates in VS Code currently view previews in their external web browser. An integrated side-panel webview would offer a seamless IDE authoring experience like react-email or Astro.

### Requirements
- VS Code extension in TypeScript.
- Automatically launches or attaches to `dotnet email-preview serve` on project detection.
- Renders the preview dashboard within a VS Code Webview panel side-by-side with the editor.
- Synchronizes cursor position with template selection.

---

## Issue 5: Add Spanish and Italian locale samples to the Gallery

**Labels**: `good first issue`, `documentation`  
**Milestone**: `v0.2.0`

### Description
The `samples/Gallery` currently includes German (`de-DE`) and French (`fr-FR`) translation dictionaries. To showcase multilingual capabilities for southern European languages, we want to add Spanish (`es-ES`) and Italian (`it-IT`).

### Requirements
- Add `samples/Gallery/Locales/es-ES.json` with keys for `greeting`, `footer.copyright`, etc.
- Add `samples/Gallery/Locales/it-IT.json`.
- Add integration assertions in `GalleryTemplateTests.cs` verifying Spanish and Italian dictionary lookups.
