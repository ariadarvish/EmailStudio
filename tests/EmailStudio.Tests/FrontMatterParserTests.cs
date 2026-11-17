using EmailStudio.Parsing;
using FluentAssertions;
using Xunit;

namespace EmailStudio.Tests;

public sealed class FrontMatterParserTests
{
    [Fact]
    public void Parse_ValidFrontMatter_ShouldExtractMetadataAndBody()
    {
        // Arrange
        var template = """
            ---
            subject: Welcome to Contoso, {Name}!
            layout: _Layouts/Base
            plainText: Please confirm your email at {ConfirmUrl}
            customKey: CustomValue
            ---
            @model MyApp.WelcomeModel
            <p>Hi @Model.Name,</p>
            """;

        // Act
        var doc = FrontMatterParser.Parse(template, "Welcome.cshtml");

        // Assert
        doc.FrontMatter.Subject.Should().Be("Welcome to Contoso, {Name}!");
        doc.FrontMatter.Layout.Should().Be("_Layouts/Base");
        doc.FrontMatter.PlainText.Should().Be("Please confirm your email at {ConfirmUrl}");
        doc.FrontMatter.CustomProperties["customKey"].Should().Be("CustomValue");
        doc.ModelType.Should().Be("MyApp.WelcomeModel");
        doc.Body.Should().Be("<p>Hi @Model.Name,</p>");
    }

    [Fact]
    public void Parse_QuotedSubject_ShouldStripQuotes()
    {
        // Arrange
        var template = """
            ---
            subject: "Your Invoice #1234 is Ready"
            ---
            <p>Invoice details</p>
            """;

        // Act
        var doc = FrontMatterParser.Parse(template);

        // Assert
        doc.FrontMatter.Subject.Should().Be("Your Invoice #1234 is Ready");
        doc.Body.Should().Be("<p>Invoice details</p>");
    }

    [Fact]
    public void Parse_UnclosedFrontMatter_ShouldThrowTemplateParseException()
    {
        // Arrange
        var template = """
            ---
            subject: Missing closing delimiter
            <p>Some text</p>
            """;

        // Act
        var act = () => FrontMatterParser.Parse(template, "Broken.cshtml");

        // Assert
        act.Should().Throw<TemplateParseException>()
            .WithMessage("*Missing terminating '---'*")
            .Where(e => e.DiagnosticCode == "TPL001");
    }

    [Fact]
    public void Parse_NoFrontMatter_ShouldReturnEmptyFrontMatter()
    {
        // Arrange
        var template = "<h1>Just HTML</h1>";

        // Act
        var doc = FrontMatterParser.Parse(template);

        // Assert
        doc.FrontMatter.Subject.Should().BeNull();
        doc.FrontMatter.Layout.Should().BeNull();
        doc.Body.Should().Be(template);
    }
}
