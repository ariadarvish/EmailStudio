# EmailStudio

> The local authoring and preview toolchain for transactional email in .NET.

[![CI](https://github.com/emailstudio/emailstudio/actions/workflows/ci.yml/badge.svg)](https://github.com/emailstudio/emailstudio/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![NuGet](https://img.shields.io/nuget/v/EmailStudio.svg)](https://www.nuget.org/packages/EmailStudio/)

EmailStudio bridges the developer experience gap for transactional emails in .NET. It compiles `.cshtml` templates into typed, zero-runtime-Razor C# render methods at compile time and runs a local preview server with instant hot reload.

## Quick Start

```bash
# 1. Install CLI preview tool
dotnet tool install -g EmailStudio.Tool

# 2. Initialize a template project
dotnet email-preview init

# 3. Start hot-reload preview server
dotnet email-preview serve
```

## Features

- **Zero Runtime Razor**: Built-time Roslyn source generator emits typed render methods (`Native AOT` friendly).
- **Compile-Time Diagnostics**: Catch missing model properties, missing subjects, or oversized emails before runtime (`TPL001`–`TPL009`).
- **Deterministic Output**: Reliable, byte-for-byte reproducible HTML output for robust snapshot testing.
- **Built-in Email Components**: Table-safe `Button`, `Heading`, `Divider`, `Spacer`, `Image`, and locale strings.
- **Local Preview Server**: Live browser previews, mobile/desktop viewports, dark mode simulation, and test SMTP forwarding.
- **Separation of Concerns**: Generates pure `EmailDraft` (Subject, Html, Text); leaves sending to MailKit, FluentEmail, or AWS SES.

## Architecture

| Project | Target | Purpose |
| --- | --- | --- |
| `EmailStudio` | `net10.0` | Core models (`EmailDraft`, `IEmailRenderer`) and render pipeline |
| `EmailStudio.Generator` | `netstandard2.0` | Roslyn incremental source generator |
| `EmailStudio.Preview` | `net10.0` | ASP.NET Core minimal API preview host |
| `EmailStudio.Tool` | `net10.0` | Global dotnet tool CLI (`dotnet email-preview`) |

## License

MIT © EmailStudio Contributors.

