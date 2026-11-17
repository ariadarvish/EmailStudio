# Sending with MailKit

EmailStudio renders an immutable `EmailDraft` that can be mapped to a MailKit `MimeMessage`:

```csharp
using MimeKit;
using MailKit.Net.Smtp;
using EmailStudio;

var draft = EmailTemplates.RenderWelcome(new WelcomeModel { Name = "Jane" });

var message = new MimeMessage();
message.From.Add(new MailboxAddress("Contoso", "noreply@contoso.com"));
message.To.Add(new MailboxAddress("Jane", "jane@example.com"));
message.Subject = draft.Subject;

var bodyBuilder = new BodyBuilder
{
    HtmlBody = draft.HtmlBody,
    TextBody = draft.TextBody
};
message.Body = bodyBuilder.ToMessageBody();

using var client = new SmtpClient();
await client.ConnectAsync("smtp.mailserver.com", 587, false);
await client.AuthenticateAsync("user", "password");
await client.SendAsync(message);
await client.DisconnectAsync(true);
```

