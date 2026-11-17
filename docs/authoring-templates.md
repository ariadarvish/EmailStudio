# Authoring Email Templates

## Anatomy of a Template

An EmailStudio template is a `.cshtml` file containing YAML front matter and Razor-compatible HTML markup:

```html
---
subject: Order #{OrderId} Confirmed
layout: _Layouts/Base
---
@model MyApp.OrderConfirmedModel

<Heading Level="1">Thank you for your order!</Heading>
<p>Your order #@Model.OrderId has been processed.</p>
<Divider />
<Button Href="@Model.TrackingUrl">Track Shipment</Button>
```

## Front Matter Fields

- `subject` (Required): String interpolated with `{ModelProperty}` syntax.
- `layout` (Optional): Relative path to a shared layout template.
- `plainText` (Optional): Explicit plain text override if auto-generation is not desired.

## Built-in Components

EmailStudio provides 6 built-in components designed for email client compatibility:
- `<Button Href="...">...</Button>`
- `<Heading Level="1|2|3">...</Heading>`
- `<Divider />`
- `<Spacer Height="20" />`
- `<Image Src="..." Alt="..." Width="..." Height="..." />`
- `<T.Try("locale.key") />`

