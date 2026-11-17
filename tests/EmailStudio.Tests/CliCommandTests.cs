using System;
using System.IO;
using EmailStudio.Tool;
using FluentAssertions;
using Xunit;

namespace EmailStudio.Tests;

public sealed class CliCommandTests : IDisposable
{
    private readonly string _testWorkspace;

    public CliCommandTests()
    {
        _testWorkspace = Path.Combine(Path.GetTempPath(), "EmailStudio_CliTest_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_testWorkspace);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_testWorkspace))
            {
                Directory.Delete(_testWorkspace, true);
            }
        }
        catch { }
    }

    [Fact]
    public void Cli_Init_ShouldCreateExpectedWorkspaceStructure()
    {
        // Act
        int exitCode = CliCommands.Init(["init", _testWorkspace]);

        // Assert
        exitCode.Should().Be(0);

        File.Exists(Path.Combine(_testWorkspace, "Templates", "_Layouts", "Base.cshtml")).Should().BeTrue();
        File.Exists(Path.Combine(_testWorkspace, "Templates", "_Partials", "Footer.cshtml")).Should().BeTrue();
        File.Exists(Path.Combine(_testWorkspace, "Templates", "Welcome.cshtml")).Should().BeTrue();
        File.Exists(Path.Combine(_testWorkspace, "Templates", "Welcome.sample.json")).Should().BeTrue();
        File.Exists(Path.Combine(_testWorkspace, "Locales", "de-DE.json")).Should().BeTrue();
        File.Exists(Path.Combine(_testWorkspace, "Locales", "fr-FR.json")).Should().BeTrue();
    }

    [Fact]
    public void Cli_Render_ShouldBatchRenderTemplatesToOutputDirectory()
    {
        // Arrange: Initialize workspace first
        CliCommands.Init(["init", _testWorkspace]);

        string outDir = Path.Combine(_testWorkspace, "dist");

        // Act
        int exitCode = CliCommands.Render(["render", _testWorkspace, "--out", outDir]);

        // Assert
        exitCode.Should().Be(0);

        File.Exists(Path.Combine(outDir, "Welcome.html")).Should().BeTrue();
        File.Exists(Path.Combine(outDir, "Welcome.txt")).Should().BeTrue();

        string html = File.ReadAllText(Path.Combine(outDir, "Welcome.html"));
        html.Should().Contain("Welcome aboard, Developer!");

        string txt = File.ReadAllText(Path.Combine(outDir, "Welcome.txt"));
        txt.Should().Contain("Welcome aboard, Developer!");
    }
}

