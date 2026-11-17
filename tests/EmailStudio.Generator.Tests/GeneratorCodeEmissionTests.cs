using FluentAssertions;
using Xunit;

namespace EmailStudio.Generator.Tests;

public sealed class GeneratorCodeEmissionTests
{
    [Fact]
    public void Generator_ShouldEmitDeterministicTypedRenderMethods()
    {
        // Arrange
        var userCode = @"
namespace MyApp.Emails {
    public sealed class WelcomeModel {
        public string Name { get; set; } = """";
        public string ActionUrl { get; set; } = """";
    }
}";

        var template = @"---
subject: Welcome to EmailStudio, {Name}!
---
@model MyApp.Emails.WelcomeModel
<p>Hello @Model.Name</p>
<Button Href=""@Model.ActionUrl"">Confirm</Button>";

        var files = new[]
        {
            new TestAdditionalText("Templates/Welcome.cshtml", template)
        };

        // Act
        var (diagnostics, emittedSource) = GeneratorTestRunner.Run(userCode, files);

        // Assert
        diagnostics.Should().BeEmpty();
        emittedSource.Should().NotBeNull();
        emittedSource.Should().Contain("public static partial class EmailTemplates");
        emittedSource.Should().Contain("RenderWelcome(global::MyApp.Emails.WelcomeModel model, string? culture = null)");
        emittedSource.Should().Contain("EmailEngine.Render(");
    }
}

