# 3. CSS Inlining Engine and Table-Based Email Components

Date: 2026-11-17

## Status

Accepted

## Context

HTML email clients (most notably Microsoft Outlook on Windows using the Word rendering engine) do not support modern CSS layouts (flexbox, grid) or `<style>` blocks in the `<head>` consistently. Styles must be inlined onto individual HTML elements, while `@media` queries must be retained for responsive clients (e.g. mobile iOS Mail, Gmail). Additionally, interactive and layout elements like buttons, spacers, and dividers require bulletproof HTML table constructs.

## Decision

1. **CSS Inlining**:
   - EmailStudio integrates `PreMailer.Net` (v2.7.4, MIT licensed, built on AngleSharp).
   - Inlining moves `<style>` rules directly into `style="..."` attributes on matching DOM elements.
   - Media queries (`@media (...) { ... }`) are preserved in a cleaned `<style>` tag in the `<head>`.
   - Pseudo-selectors (e.g. `:hover`) are kept or warned appropriately.

2. **Built-in Components**:
   - Six built-in email components are provided: `Button`, `Heading`, `Divider`, `Spacer`, `Image`, and `T.Try`.
   - `Button` expands to an email-safe table wrapper with inline padding, background-color, and border-radius.
   - `Spacer` uses Outlook-safe line-height and height rules (`mso-line-height-rule: exactly`).
   - `Divider` outputs a standardized horizontal rule with normalized borders.
   - `Heading` applies normalized margin and font hierarchies.
   - `Image` enforces `display: block; border: none; outline: none`.

## Consequences

- Templates can be authored using clean, semantic tags and standard CSS classes, which are automatically transformed into email-safe markup.
- Full compatibility with major email clients without manual HTML table boilerplate.
