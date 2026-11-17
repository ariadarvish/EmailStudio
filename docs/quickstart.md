# Quick Start Guide

Get started with EmailStudio in three simple steps.

## 1. Installation

Install the preview CLI tool globally:

```bash
dotnet tool install -g EmailStudio.Tool
```

Install the runtime package into your application:

```bash
dotnet add package EmailStudio
```

## 2. Author a Template

Create `Templates/Welcome.cshtml`:

```html
---
subject: Welcome to EmailStudio, {Name}!
---
@model MyApp.WelcomeModel

<p>Hi @Model.Name,</p>
<Button Href="@Model.ConfirmUrl">Confirm Your Email</Button>
```

Add sample preview data in `Templates/Welcome.sample.json`:

```json
{
  "Name": "Aria",
  "ConfirmUrl": "https://example.com/confirm?token=xyz"
}
```

## 3. Preview with Hot Reload

Start the local preview server:

```bash
dotnet email-preview serve
```

Open `http://localhost:5173` to view the live preview with desktop and mobile viewport toggles.

