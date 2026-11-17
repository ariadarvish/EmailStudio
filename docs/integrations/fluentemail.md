# Sending with FluentEmail

Integrating EmailStudio with FluentEmail is straightforward using `EmailDraft`:

```csharp
using FluentEmail.Core;
using EmailStudio;

var draft = EmailTemplates.RenderWelcome(new WelcomeModel { Name = "Jane" });

await fluentEmail
    .To("jane@example.com", "Jane")
    .Subject(draft.Subject)
    .Body(draft.HtmlBody, isHtml: true)
    .PlaintextAlternativeBody(draft.TextBody)
    .SendAsync();
```

