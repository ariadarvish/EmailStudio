# EmailStudio

> The local authoring and preview toolchain for transactional email in .NET. ("react-email for .NET")

[![CI](https://github.com/ariadarvish/emailstudio/actions/workflows/release.yml/badge.svg)](https://github.com/ariadarvish/emailstudio/actions/workflows/release.yml)
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
# Option A: In this repository (direct run without installing):
dotnet run --project src/EmailStudio.Tool -- init
dotnet run --project src/EmailStudio.Tool -- serve samples/Gallery

# Option B: Global tool (once published or installed locally):
dotnet tool install -g EmailStudio.Tool
dotnet email-preview init
dotnet email-preview serve
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

---

## 🚀 Getting Started (New to this Repository?)

Follow these steps to clone, build, and explore the project in under 60 seconds:

### Prerequisites
- [.NET 10 or .NET 11 SDK](https://dotnet.microsoft.com/download)
- Optional: Visual Studio 2026+ or VS Code with C# Dev Kit

### Step 1: Clone the Repository
```bash
git clone https://github.com/emailstudio/emailstudio.git
cd EmailStudio
```

### Step 2: Build and Test
Verify that everything builds and tests pass cleanly:
```bash
dotnet test
```

### Step 3: Run the Live Preview Server
You have two convenient ways to run the preview server:

#### Option A: Via Command Line (Terminal)
Run the gallery directly from the repository root:
```bash
dotnet run --project src/EmailStudio.Tool -- serve samples/Gallery
```
Now open **http://localhost:5055** in your browser.

#### Option B: Via Visual Studio (F5)
1. Open `EmailStudio.slnx` in Visual Studio.
2. Ensure **`EmailStudio.Preview`** (or **`EmailStudio.Tool`**) is selected in the Startup Projects dropdown.
3. Press **F5** (or click the green **Play** button).
4. Visual Studio will launch Kestrel and open your browser to **http://localhost:5055** automatically with the 10 gallery templates loaded.

### Step 4: Try Live Hot Reload
1. Keep the browser open at `http://localhost:5055`.
2. Open `samples/Gallery/Templates/Welcome.cshtml` in your editor.
3. Change `<Heading level="1">` or adjust a button background color and save the file.
4. Watch the browser instantly update in < 1 second over WebSockets!

---

## 📦 Client Integration Guide (Add to Your Own Project)

Follow this step-by-step guide to use EmailStudio in your existing .NET 10 or 11 web API, worker service, or backend app.

### Step 1: Install the NuGet Package
In your application project directory, install `EmailStudio`:
```bash
dotnet add package EmailStudio
```
*(This single package automatically brings in both the runtime engine and the Roslyn incremental source generator).*

Optionally, install the global CLI preview tool for local development:
```bash
dotnet tool install -g EmailStudio.Tool
```

### Step 2: Scaffold Templates
Run the `init` command in your project directory (or create a `Templates/` folder):
```bash
dotnet email-preview init
```
This scaffolds:
```
YourApp/
├── Templates/
│   ├── _Layouts/
│   │   └── Base.cshtml
│   ├── _Partials/
│   │   └── Footer.cshtml
│   ├── Welcome.cshtml
│   └── Welcome.sample.json
└── Locales/
    └── de-DE.json
```

### Step 3: Author Your Email Template
Create or edit `Templates/Welcome.cshtml`:

```html
---
subject: Welcome to Contoso, @Model.UserName!
preheader: Your developer workspace is ready.
tags: [onboarding]
---
@model YourApp.Models.WelcomeEmailModel

<Heading level="1">Welcome aboard, @Model.UserName!</Heading>
<p>Thank you for joining Contoso. Your organization workspace is now configured.</p>

<Button href="@Model.ActionUrl" background="#2563eb" color="#ffffff">
    Open Your Dashboard
</Button>

<Divider spacing="24px" />
<Include path="_Partials/Footer.cshtml" />
```

Create sample mock data in `Templates/Welcome.sample.json` so you can preview it immediately:
```json
{
  "UserName": "Alex Morgan",
  "ActionUrl": "https://contoso.com/get-started"
}
```

### Step 4: Live Preview While Authoring
Launch the live preview server while writing your templates:
```bash
dotnet email-preview serve
```
Open `http://localhost:5055` to view desktop (700px) and mobile (375px) renders, dark-mode preview, and real-time syntax error diagnostics.

### Step 5: Render Strongly-Typed Emails in C#
When you build your project (`dotnet build`), the Roslyn source generator generates typed render methods on `EmailTemplates`. Call them directly with zero runtime Razor overhead:

```csharp
using EmailStudio;
using YourApp.Models;

public class NotificationService
{
    public async Task SendWelcomeEmailAsync(string email, string name)
    {
        var model = new WelcomeEmailModel(name, "https://app.contoso.com/login");

        // Zero-runtime Razor: Generated at compile time, Native AOT friendly!
        EmailDraft draft = EmailTemplates.RenderWelcome(model, culture: "en-US");

        Console.WriteLine(draft.Subject);   // "Welcome to Contoso, Alex Morgan!"
        Console.WriteLine(draft.HtmlBody);  // CSS-inlined, responsive HTML
        Console.WriteLine(draft.PlainText); // Clean markdown-like text

        // Send using your preferred email provider (MailKit, SendGrid, SES, FluentEmail):
        await DispatchEmailAsync(email, draft);
    }
}
```

### Step 6: Dispatch with Any Sender (e.g. MailKit / MimeKit)
EmailStudio produces pure `EmailDraft` instances, leaving transmission to your choice of provider:

```csharp
using MimeKit;
using MailKit.Net.Smtp;

async Task DispatchEmailAsync(string recipientEmail, EmailDraft draft)
{
    var message = new MimeMessage();
    message.From.Add(new MailboxAddress("Contoso", "noreply@contoso.com"));
    message.To.Add(new MailboxAddress("", recipientEmail));
    message.Subject = draft.Subject;

    var builder = new BodyBuilder
    {
        HtmlBody = draft.HtmlBody,
        TextBody = draft.PlainText
    };
    message.Body = builder.ToMessageBody();

    using var client = new SmtpClient();
    await client.ConnectAsync("smtp.mailgun.org", 587, false);
    await client.AuthenticateAsync("apiKey", "secret");
    await client.SendAsync(message);
    await client.DisconnectAsync(true);
}
```

### Step 7: Automated Snapshot Testing (Optional)
Because EmailStudio output is 100% deterministic, you can add snapshot regression tests with `Verify.Xunit`:

```csharp
[Fact]
public Task WelcomeEmail_ShouldMatchSnapshot()
{
    var model = new WelcomeEmailModel("Test User", "https://example.com");
    var draft = EmailTemplates.RenderWelcome(model);

    return Verify(new { draft.Subject, draft.HtmlBody, draft.PlainText });
}
```

---

## Architecture

| Project | Target | Purpose |
| --- | --- | --- |
| `EmailStudio` | `net10.0;net11.0` | Core models (`EmailDraft`, `IEmailRenderer`) and render pipeline |
| `EmailStudio.Generator` | `netstandard2.0` | Roslyn incremental source generator |
| `EmailStudio.Preview` | `net10.0;net11.0` | ASP.NET Core minimal API preview host |
| `EmailStudio.Tool` | `net11.0` | Global dotnet tool CLI (`dotnet email-preview`) |

## License

MIT © EmailStudio Contributors.

