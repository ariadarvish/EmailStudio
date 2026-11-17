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
    /// <param name="args">Command line arguments.</param>
    /// <param name="templateDirectory">Path to the directory containing templates.</param>
    /// <returns>A configured <see cref="WebApplication"/>.</returns>
    public static WebApplication CreateApp(string[]? args = null, string? templateDirectory = null)
    {
        var builder = WebApplication.CreateBuilder(args ?? []);
        builder.Services.AddEmailStudio();

        var app = builder.Build();

        app.MapGet("/health", () => Results.Ok(new
        {
            Status = "Healthy",
            TemplateDirectory = templateDirectory ?? "Templates",
            Version = "0.1.0-alpha"
        }));

        return app;
    }
}

