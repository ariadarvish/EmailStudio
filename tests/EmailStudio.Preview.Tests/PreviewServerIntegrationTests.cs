using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using EmailStudio.Preview;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EmailStudio.Preview.Tests;

public sealed class PreviewServerIntegrationTests : IAsyncLifetime
{
    private readonly string _tempDir;
    private readonly string _templatesDir;
    private readonly string _localesDir;
    private Microsoft.AspNetCore.Builder.WebApplication? _app;
    private HttpClient? _client;
    private string? _baseUrl;

    public PreviewServerIntegrationTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "EmailStudio_PreviewTest_" + Guid.NewGuid().ToString("N"));
        _templatesDir = Path.Combine(_tempDir, "Templates");
        _localesDir = Path.Combine(_tempDir, "Locales");

        Directory.CreateDirectory(_templatesDir);
        Directory.CreateDirectory(_localesDir);

        // Seed sample template and sample data
        File.WriteAllText(Path.Combine(_templatesDir, "Welcome.cshtml"), """
            ---
            subject: Welcome to Contoso, {Name}!
            ---
            <h1>Hello @Model.Name</h1>
            <Button Href="https://example.com/start">Get Started</Button>
            """);

        File.WriteAllText(Path.Combine(_templatesDir, "Welcome.sample.json"), """
            {
                "Name": "Sarah"
            }
            """);

        File.WriteAllText(Path.Combine(_localesDir, "de-DE.json"), """
            {
                "greeting": "Guten Tag"
            }
            """);
    }

    public async Task InitializeAsync()
    {
        var options = new PreviewServerOptions
        {
            TemplateDirectory = _templatesDir,
            LocalesDirectory = _localesDir,
            SmtpHost = "127.0.0.1",
            SmtpPort = 1025
        };

        _app = PreviewServer.CreateApp(serverOptions: options);
        _app.Urls.Add("http://127.0.0.1:0");
        await _app.StartAsync();

        var serverAddress = _app.Urls.GetEnumerator();
        serverAddress.MoveNext();
        _baseUrl = serverAddress.Current;

        _client = new HttpClient { BaseAddress = new Uri(_baseUrl) };
    }

    public async Task DisposeAsync()
    {
        _client?.Dispose();
        if (_app != null)
        {
            await _app.StopAsync();
            await _app.DisposeAsync();
        }

        try
        {
            if (Directory.Exists(_tempDir))
            {
                Directory.Delete(_tempDir, true);
            }
        }
        catch { }
    }

    [Fact]
    public async Task DashboardEndpoint_ShouldReturnHtmlUi()
    {
        // Act
        var response = await _client!.GetAsync("/");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("text/html");
        var html = await response.Content.ReadAsStringAsync();
        html.Should().Contain("EmailStudio");
        html.Should().Contain("Preview");
        html.Should().Contain("Desktop (700px)");
        html.Should().Contain("Mobile (375px)");
    }

    [Fact]
    public async Task ApiTemplates_ShouldReturnTemplateList()
    {
        // Act
        var response = await _client!.GetAsync("/api/templates");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        doc.RootElement.GetArrayLength().Should().Be(1);
        doc.RootElement[0].GetProperty("name").GetString().Should().Be("Welcome");
        doc.RootElement[0].GetProperty("hasSampleData").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task ApiCultures_ShouldReturnAvailableCultures()
    {
        // Act
        var response = await _client!.GetAsync("/api/cultures");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("de-DE");
    }

    [Fact]
    public async Task PreviewEndpoint_ShouldRenderTemplateHtml()
    {
        // Act
        var response = await _client!.GetAsync("/preview/Welcome");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var html = await response.Content.ReadAsStringAsync();
        html.Should().Contain("Hello Sarah");
        html.Should().Contain("https://example.com/start");
        html.Should().Contain("role=\"presentation\""); // Expanded button component
    }

    [Fact]
    public async Task ApiTemplateMetadata_ShouldReturnSubjectAndBodies()
    {
        // Act
        var response = await _client!.GetAsync("/api/template/Welcome");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("success").GetBoolean().Should().BeTrue();
        doc.RootElement.GetProperty("draft").GetProperty("subject").GetString().Should().Be("Welcome to Contoso, Sarah!");
    }

    [Fact]
    public async Task WebSocketEndpoint_ShouldReceiveReloadMessage()
    {
        // Arrange
        var wsUri = new Uri(_baseUrl!.Replace("http://", "ws://") + "/ws");
        using var wsClient = new ClientWebSocket();
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        await wsClient.ConnectAsync(wsUri, cts.Token);
        wsClient.State.Should().Be(WebSocketState.Open);

        // Act: Trigger broadcast via WebSocketHub directly
        var hub = _app!.Services.GetRequiredService<WebSocketHub>();
        await hub.BroadcastReloadAsync("Welcome");

        // Assert: Read message from WebSocket
        var buffer = new byte[1024];
        var receiveResult = await wsClient.ReceiveAsync(new ArraySegment<byte>(buffer), cts.Token);
        receiveResult.MessageType.Should().Be(WebSocketMessageType.Text);

        string receivedText = Encoding.UTF8.GetString(buffer, 0, receiveResult.Count);
        receivedText.Should().Contain("reload");
        receivedText.Should().Contain("Welcome");

        await wsClient.CloseAsync(WebSocketCloseStatus.NormalClosure, "Done", cts.Token);
    }
}
