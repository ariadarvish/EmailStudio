# EmailStudio

> The local authoring and preview toolchain for transactional email in .NET. ("react-email for .NET")

[![CI](https://github.com/emailstudio/emailstudio/actions/workflows/ci.yml/badge.svg)](https://github.com/emailstudio/emailstudio/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![NuGet](https://img.shields.io/nuget/v/EmailStudio.svg)](https://www.nuget.org/packages/EmailStudio/)

EmailStudio bridges the developer experience gap for transactional emails in .NET. It compiles `.cshtml` templates into typed, zero-runtime-Razor C# render methods at compile time and runs a local preview server with instant hot reload.

```
┌────────────────────────────────────────────────────────────────────────┐
│ EmailStudio Local Preview - http://localhost:5055                     │
├───────────────┬────────────────────────────────────────────────────────┤
│ Templates (10)│ Viewport: [ Desktop (700px) ] [ Mobile (375px) ]       │
│ > Welcome     │ Dark Mode: [OFF]  Culture: [ en-US (default) v ]       │
│   ResetPass   ├────────────────────────────────────────────────────────┤
│   InvoicePaid │ Welcome to EmailStudio!                                │
│   InvoiceOver │                                                        │
│   TeamInvite  │ Welcome, Jane!                                         │
│   PassChanged │ Your workspace is ready. Click below to begin.         │
│   WeeklyDigest│                                                        │
│   ShippingNot │            [ Open Workspace ]                          │
│   AccountDel  │                                                        │
│   TwoFactorEn │ © 2026 EmailStudio. All rights reserved.               │
└───────────────┴────────────────────────────────────────────────────────┘
```

## Quick Start

```bash
# 1. Install CLI preview tool
dotnet tool install -g EmailStudio.Tool

# 2. Initialize a template project
dotnet email-preview init

# 3. Start hot-reload preview server
dotnet email-preview serve

# 4. Or statically render templates to disk
dotnet email-preview render . --out dist --culture en-US
```

## Features

- **Zero Runtime Razor**: Build-time Roslyn source generator emits typed, dependency-free C# render methods (`Native AOT` compatible).
- **Compile-Time Diagnostics**: Catch missing model properties, missing subjects, or oversized emails before runtime (`TPL001`–`TPL009`).
- **Deterministic Output**: Reliable, byte-for-byte reproducible HTML output for robust snapshot testing.
- **Built-in Email Components**: Table-safe `<Button>`, `<Heading>`, `<Divider>`, `<Spacer>`, `<Image>`, and `<T.Try>`.
- **PreMailer CSS Inlining**: Inlines standard styles while preserving responsive `@media` queries in the `<head>`.
- **Local Preview Server**: Live browser previews, mobile (375px)/desktop (700px) viewports, dark mode simulation, live syntax errors, and test SMTP forwarding (e.g. Mailpit/Inbucket on port 1025).
- **Separation of Concerns**: Generates pure `EmailDraft` (Subject, Html, Text); leaves delivery to MailKit, FluentEmail, or AWS SES.

## Target Frameworks

- `EmailStudio`: Multi-targets `net10.0;net11.0`
- `EmailStudio.Preview`: Multi-targets `net10.0;net11.0`
- `EmailStudio.Tool`: Targets `net11.0`
- `EmailStudio.Generator`: Roslyn incremental source generator targeting `netstandard2.0`

## CLI Commands

| Command | Description | Example |
| --- | --- | --- |
| `init [path]` | Scaffold a starter email template project with layout, partials, and samples | `dotnet email-preview init ./MyEmails` |
| `serve [path] [--port <p>]` | Launch hot-reload preview server with live dashboard | `dotnet email-preview serve ./MyEmails --port 5055` |
| `render [path] [--out <dir>]` | Render all templates statically to HTML and plain text files | `dotnet email-preview render ./MyEmails --out ./dist` |

## Template Example

Templates live in `Templates/**/*.cshtml` and include YAML frontmatter, typed `@model` bindings, and custom components:

```html
---
subject: Welcome to EmailStudio, @Model.Name!
preheader: Your developer workspace is ready.
tags: [onboarding, transactional]
---
@model EmailStudio.Samples.WelcomeModel

<Heading level="1">Welcome, @Model.Name!</Heading>
<p>Thank you for signing up for EmailStudio.</p>
<Button href="@Model.ActionUrl" background="#2563eb" color="#ffffff">Open Dashboard</Button>
<Divider />
<Include path="Partials/_Footer.cshtml" />
```

At compile time, `EmailStudio.Generator` generates:

```csharp
public static partial class EmailTemplates
{
    public static EmailDraft RenderWelcome(EmailStudio.Samples.WelcomeModel model, string? culture = null);
}
```

Rendering an email in your app:

```csharp
var draft = EmailTemplates.RenderWelcome(new WelcomeModel("Jane", "https://app.example.com"));
Console.WriteLine(draft.Subject);   // "Welcome to EmailStudio, Jane!"
Console.WriteLine(draft.Html);      // Responsive, CSS-inlined HTML
Console.WriteLine(draft.PlainText); // Clean markdown-like plain text
```

## Sample Gallery

The repository includes a ready-to-run gallery in `samples/Gallery` featuring 10 production-ready transactional templates:
1. `Welcome`: Account activation with hero CTA button
2. `ResetPassword`: Security reset link with timeout note
3. `InvoicePaid`: Receipt with billing breakdown table
4. `InvoiceOverdue`: Urgent payment reminder notice
5. `TeamInvite`: Collaborative organization invitation
6. `PasswordChanged`: Security confirmation alert
7. `WeeklyDigest`: Multi-item summary cards and statistics
8. `ShippingNotification`: Order tracking details and package ETA
9. `AccountDeleted`: Account closure confirmation
10. `TwoFactorEnabled`: Security notice with 2FA backup codes

To view the gallery in the preview server:
```bash
dotnet run --project src/EmailStudio.Tool -- serve samples/Gallery
```

## Architecture

| Project | Target | Purpose |
| --- | --- | --- |
| `EmailStudio` | `net10.0;net11.0` | Core models (`EmailDraft`, `IEmailRenderer`) and render pipeline |
| `EmailStudio.Generator` | `netstandard2.0` | Roslyn incremental source generator |
| `EmailStudio.Preview` | `net10.0;net11.0` | ASP.NET Core minimal API preview host |
| `EmailStudio.Tool` | `net11.0` | Global dotnet tool CLI (`dotnet email-preview`) |

## License

MIT © EmailStudio Contributors.

