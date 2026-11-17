using System;
using System.IO;
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EmailStudio.Preview;

/// <summary>
/// Configures and starts the EmailStudio preview server.
/// </summary>
public static class PreviewServer
{
    /// <summary>
    /// Creates and configures the ASP.NET Core web application for previewing templates.
    /// </summary>
    public static WebApplication CreateApp(string[]? args = null, PreviewServerOptions? serverOptions = null)
    {
        serverOptions ??= new PreviewServerOptions();

        // Parse any CLI overrides
        if (args != null)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] is "--port" or "-p" && i + 1 < args.Length && int.TryParse(args[i + 1], out int p))
                {
                    serverOptions.Port = p;
                }
                else if (args[i] is "--project" or "--dir" && i + 1 < args.Length)
                {
                    serverOptions.TemplateDirectory = Path.Combine(args[i + 1], "Templates");
                    serverOptions.LocalesDirectory = Path.Combine(args[i + 1], "Locales");
                }
                else if (args[i] is "--smtp" && i + 1 < args.Length)
                {
                    string[] parts = args[i + 1].Split(':');
                    serverOptions.SmtpHost = parts[0];
                    if (parts.Length > 1 && int.TryParse(parts[1], out int sp))
                    {
                        serverOptions.SmtpPort = sp;
                    }
                }
            }
        }

        var builder = WebApplication.CreateBuilder();

        // Register core preview services
        var hub = new WebSocketHub();
        var templateService = new TemplateService(serverOptions);
        var watcher = new TemplateWatcher(serverOptions, hub, templateService);

        builder.Services.AddSingleton(serverOptions);
        builder.Services.AddSingleton(hub);
        builder.Services.AddSingleton(templateService);
        builder.Services.AddSingleton(watcher);
        builder.Services.AddEmailStudio();

        var app = builder.Build();

        app.UseWebSockets();

        // Health endpoint
        app.MapGet("/health", () => Results.Ok(new
        {
            Status = "Healthy",
            TemplateDirectory = serverOptions.TemplateDirectory,
            Version = "0.1.0-alpha"
        }));

        // Developer Dashboard UI
        app.MapGet("/", () => Results.Content(PreviewDashboardHtml.Content, "text/html"));

        // Favicon Endpoints
        app.MapGet("/favicon.svg", () => Results.Content(PreviewDashboardHtml.FaviconSvg, "image/svg+xml"));
        app.MapGet("/favicon.ico", () => Results.Bytes(PreviewDashboardHtml.FaviconIcoBytes, "image/x-icon"));

        // Template List API
        app.MapGet("/api/templates", (TemplateService svc) => Results.Ok(svc.GetTemplates()));

        // Available Cultures API
        app.MapGet("/api/cultures", (TemplateService svc) => Results.Ok(svc.GetAvailableCultures()));

        // Template Metadata & Status API
        app.MapGet("/api/template/{name}", (string name, string? culture, TemplateService svc) =>
        {
            var result = svc.Render(name, culture);
            return Results.Ok(result);
        });

        // Rendered Iframe Content
        app.MapGet("/preview/{name}", (string name, string? culture, TemplateService svc) =>
        {
            var result = svc.Render(name, culture);
            if (result.Success && result.Draft != null)
            {
                return Results.Content(result.Draft.HtmlBody, "text/html");
            }

            string errHtml = $"""
                <html>
                <body style="font-family:sans-serif;padding:32px;background:#fef2f2;color:#991b1b;">
                    <h2>Rendering Failed: [{result.Diagnostic?.Code ?? "TPL001"}]</h2>
                    <p style="font-size:16px;">{result.Diagnostic?.Message}</p>
                    <pre style="background:#fee2e2;padding:16px;border-radius:6px;overflow:auto;">{result.Diagnostic?.FilePath}:{result.Diagnostic?.LineNumber}</pre>
                </body>
                </html>
                """;

            return Results.Content(errHtml, "text/html");
        });

        // Send to Inbox (SMTP)
        app.MapPost("/api/send/{name}", async (string name, HttpRequest request, TemplateService svc, PreviewServerOptions opts) =>
        {
            using var reader = new StreamReader(request.Body);
            string body = await reader.ReadToEndAsync();
            string to = "test@example.com";
            string? culture = null;

            if (!string.IsNullOrWhiteSpace(body))
            {
                try
                {
                    using var doc = JsonDocument.Parse(body);
                    if (doc.RootElement.TryGetProperty("to", out var toProp))
                    {
                        to = toProp.GetString() ?? to;
                    }
                    if (doc.RootElement.TryGetProperty("culture", out var culProp))
                    {
                        culture = culProp.GetString();
                    }
                }
                catch { }
            }

            var renderResult = svc.Render(name, culture);
            if (!renderResult.Success || renderResult.Draft == null)
            {
                return Results.BadRequest(new { Success = false, Message = renderResult.Diagnostic?.Message ?? "Render failed" });
            }

            var (success, msg) = await SmtpSender.SendAsync(opts.SmtpHost, opts.SmtpPort, to, renderResult.Draft);
            return Results.Ok(new { Success = success, Message = msg });
        });

        // WebSocket Hot Reload Endpoint
        app.MapGet("/ws", async (HttpContext context, WebSocketHub wsHub) =>
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                var clientId = Guid.NewGuid();
                wsHub.AddClient(clientId, webSocket);

                var buffer = new byte[1024 * 4];
                try
                {
                    while (webSocket.State == WebSocketState.Open)
                    {
                        var receiveResult = await webSocket.ReceiveAsync(
                            new ArraySegment<byte>(buffer),
                            CancellationToken.None);

                        if (receiveResult.MessageType == WebSocketMessageType.Close)
                        {
                            await webSocket.CloseAsync(
                                WebSocketCloseStatus.NormalClosure,
                                "Closing",
                                CancellationToken.None);
                            break;
                        }
                    }
                }
                finally
                {
                    wsHub.RemoveClient(clientId);
                }
            }
            else
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        });

        return app;
    }
}
