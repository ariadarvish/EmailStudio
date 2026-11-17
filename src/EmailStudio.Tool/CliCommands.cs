using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using EmailStudio.Preview;

namespace EmailStudio.Tool;

public static class CliCommands
{
    public static int Init(string[] args)
    {
        string targetDir = args.Length > 1 && !args[1].StartsWith("-") ? args[1] : ".";
        string templatesDir = Path.Combine(targetDir, "Templates");
        string layoutsDir = Path.Combine(templatesDir, "_Layouts");
        string partialsDir = Path.Combine(templatesDir, "_Partials");
        string localesDir = Path.Combine(targetDir, "Locales");

        Directory.CreateDirectory(layoutsDir);
        Directory.CreateDirectory(partialsDir);
        Directory.CreateDirectory(localesDir);

        // 1. Base Layout
        string baseLayout = """
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset="utf-8">
                <meta name="viewport" content="width=device-width, initial-scale=1.0">
                <style>
                    body { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif; background-color: #f8fafc; margin: 0; padding: 24px; color: #334155; }
                    .container { max-width: 600px; margin: 0 auto; background: #ffffff; border-radius: 8px; padding: 32px; box-shadow: 0 4px 6px -1px rgba(0,0,0,0.1); }
                    .footer { margin-top: 32px; padding-top: 16px; border-top: 1px solid #e2e8f0; font-size: 12px; color: #94a3b8; text-align: center; }
                </style>
            </head>
            <body>
                <div class="container">
                    @RenderBody()
                    <div class="footer">
                        <Partial Name="_Partials/Footer" />
                    </div>
                </div>
            </body>
            </html>
            """;
        File.WriteAllText(Path.Combine(layoutsDir, "Base.cshtml"), baseLayout);

        // 2. Footer Partial
        string footerPartial = """
            <p style="margin: 0;">© 2026 Your Company. All rights reserved.</p>
            <p style="margin: 4px 0 0 0;"><T.Try Key="footer.note" /></p>
            """;
        File.WriteAllText(Path.Combine(partialsDir, "Footer.cshtml"), footerPartial);

        // 3. Welcome Template
        string welcomeTemplate = """
            ---
            subject: Welcome to EmailStudio, {Name}!
            layout: _Layouts/Base
            ---
            @model MyApp.Emails.WelcomeModel

            <Heading Level="1">Welcome aboard, @Model.Name!</Heading>
            <p>Thanks for joining us. Please confirm your email address below:</p>
            <Spacer Height="20" />
            <Button Href="@Model.ConfirmUrl" BgColor="#2563eb">Confirm Email</Button>
            <Spacer Height="20" />
            <Divider />
            <p style="font-size: 13px; color: #64748b;">This link will expire in 24 hours.</p>
            """;
        File.WriteAllText(Path.Combine(templatesDir, "Welcome.cshtml"), welcomeTemplate);
        File.WriteAllText(Path.Combine(templatesDir, "Welcome.sample.json"), "{\n  \"Name\": \"Developer\",\n  \"ConfirmUrl\": \"https://example.com/confirm?token=xyz\"\n}");

        // 4. ResetPassword Template
        string resetTemplate = """
            ---
            subject: Reset your password, {Name}
            layout: _Layouts/Base
            ---
            @model MyApp.Emails.ResetPasswordModel

            <Heading Level="1">Password Reset</Heading>
            <p>Hello @Model.Name,</p>
            <p>We received a request to reset your password. Click the button below to proceed:</p>
            <Spacer Height="20" />
            <Button Href="@Model.ResetUrl" BgColor="#0f172a">Reset Password</Button>
            <Spacer Height="20" />
            <Divider />
            <p style="font-size: 13px; color: #64748b;">If you did not request this, you can ignore this email.</p>
            """;
        File.WriteAllText(Path.Combine(templatesDir, "ResetPassword.cshtml"), resetTemplate);
        File.WriteAllText(Path.Combine(templatesDir, "ResetPassword.sample.json"), "{\n  \"Name\": \"Alex\",\n  \"ResetUrl\": \"https://example.com/reset?token=123\"\n}");

        // 5. InvoicePaid Template
        string invoiceTemplate = """
            ---
            subject: Receipt for Invoice #{InvoiceNumber}
            layout: _Layouts/Base
            ---
            @model MyApp.Emails.InvoiceModel

            <Heading Level="1">Payment Receipt</Heading>
            <p>Hi @Model.CustomerName,</p>
            <p>Thank you for your payment of <strong>@Model.Amount</strong> for invoice <strong>#{InvoiceNumber}</strong>.</p>
            <Spacer Height="20" />
            <Button Href="@Model.InvoiceUrl" BgColor="#10b981">View Receipt</Button>
            """;
        File.WriteAllText(Path.Combine(templatesDir, "InvoicePaid.cshtml"), invoiceTemplate);
        File.WriteAllText(Path.Combine(templatesDir, "InvoicePaid.sample.json"), "{\n  \"CustomerName\": \"Contoso Inc\",\n  \"InvoiceNumber\": \"INV-1001\",\n  \"Amount\": \"$49.00\",\n  \"InvoiceUrl\": \"https://example.com/invoices/1001\"\n}");

        // 6. Locales
        File.WriteAllText(Path.Combine(localesDir, "de-DE.json"), "{\n  \"footer.note\": \"Bitte antworten Sie nicht auf diese E-Mail.\"\n}");
        File.WriteAllText(Path.Combine(localesDir, "fr-FR.json"), "{\n  \"footer.note\": \"Veuillez ne pas répondre à cet e-mail.\"\n}");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Initialized EmailStudio workspace in '{targetDir}'.");
        Console.ResetColor();
        Console.WriteLine("Created:");
        Console.WriteLine("  Templates/_Layouts/Base.cshtml");
        Console.WriteLine("  Templates/_Partials/Footer.cshtml");
        Console.WriteLine("  Templates/Welcome.cshtml (.sample.json)");
        Console.WriteLine("  Templates/ResetPassword.cshtml (.sample.json)");
        Console.WriteLine("  Templates/InvoicePaid.cshtml (.sample.json)");
        Console.WriteLine("  Locales/de-DE.json, fr-FR.json");
        Console.WriteLine();
        Console.WriteLine("Next steps:");
        Console.WriteLine("  dotnet email-preview serve");
        return 0;
    }

    public static async Task<int> ServeAsync(string[] args)
    {
        var options = new PreviewServerOptions();
        var app = PreviewServer.CreateApp(args, options);

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"EmailStudio Preview Server starting on http://{options.Host}:{options.Port}");
        Console.ResetColor();
        Console.WriteLine($"Watching: {Path.GetFullPath(options.TemplateDirectory)}");
        Console.WriteLine($"SMTP Catcher: {options.SmtpHost}:{options.SmtpPort}");
        Console.WriteLine("Press Ctrl+C to stop.");

        await app.RunAsync();
        return 0;
    }

    public static int Render(string[] args)
    {
        string projectPath = ".";
        string outDir = "dist";
        string? culture = null;

        for (int i = 1; i < args.Length; i++)
        {
            if (args[i] is "--out" or "-o" && i + 1 < args.Length)
            {
                outDir = args[i + 1];
                i++;
            }
            else if (args[i] is "--culture" or "-c" && i + 1 < args.Length)
            {
                culture = args[i + 1];
                i++;
            }
            else if (!args[i].StartsWith("-"))
            {
                projectPath = args[i];
            }
        }

        string templatesDir = Directory.Exists(Path.Combine(projectPath, "Templates"))
            ? Path.Combine(projectPath, "Templates")
            : projectPath;

        string localesDir = Path.Combine(projectPath, "Locales");

        var serverOptions = new PreviewServerOptions
        {
            TemplateDirectory = templatesDir,
            LocalesDirectory = localesDir
        };

        var service = new TemplateService(serverOptions);
        var templates = service.GetTemplates();

        if (templates.Count == 0)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"No templates found in '{templatesDir}'.");
            Console.ResetColor();
            return 0;
        }

        Directory.CreateDirectory(outDir);

        Console.WriteLine($"Rendering {templates.Count} template(s) to '{outDir}' (Culture: {culture ?? "default"})...");
        Console.WriteLine(new string('-', 75));
        Console.WriteLine($"{"Template",-25} {"Status",-8} {"Size (KB)",-12} {"Subject"}");
        Console.WriteLine(new string('-', 75));

        bool hasErrors = false;

        foreach (var t in templates)
        {
            var result = service.Render(t.Name, culture);
            if (result.Success && result.Draft != null)
            {
                int byteLength = Encoding.UTF8.GetByteCount(result.Draft.HtmlBody);
                double kb = byteLength / 1024.0;

                string htmlFile = Path.Combine(outDir, $"{t.Name}.html");
                string txtFile = Path.Combine(outDir, $"{t.Name}.txt");

                File.WriteAllText(htmlFile, result.Draft.HtmlBody, Encoding.UTF8);
                File.WriteAllText(txtFile, result.Draft.TextBody, Encoding.UTF8);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write($"{$"{t.Name}",-25} {"PASS",-8} ");
                Console.ResetColor();
                Console.WriteLine($"{kb,8:F1} KB   {result.Draft.Subject}");
            }
            else
            {
                hasErrors = true;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write($"{$"{t.Name}",-25} {"FAIL",-8} ");
                Console.ResetColor();
                Console.WriteLine($"         [{result.Diagnostic?.Code}] {result.Diagnostic?.Message}");
            }
        }

        Console.WriteLine(new string('-', 75));
        if (hasErrors)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Render failed: One or more templates failed validation or exceeded size limits.");
            Console.ResetColor();
            return 1;
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("All templates rendered successfully within size limits.");
        Console.ResetColor();
        return 0;
    }
}

