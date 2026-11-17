using System.Linq;
using FluentAssertions;
using Xunit;

namespace EmailStudio.Generator.Tests;

public sealed class GeneratorDiagnosticsTests
{
    private const string EmptyUserCode = @"namespace TestApp { public class Dummy {} }";

    [Fact]
    public void Generator_TPL001_ShouldBeEmittedOnUnclosedFrontMatter()
    {
        // Arrange
        var files = new[]
        {
            new TestAdditionalText("Templates/Welcome.cshtml", "---\nsubject: Test\n<p>No closing delimiter</p>")
        };

        // Act
        var (diagnostics, _) = GeneratorTestRunner.Run(EmptyUserCode, files);

        // Assert
        diagnostics.Should().Contain(d => d.Id == "TPL001");
    }

    [Fact]
    public void Generator_TPL002_ShouldBeEmittedWhenModelPropertyDoesNotExist()
    {
        // Arrange
        var userCode = @"
namespace MyApp {
    public class UserModel {
        public string Name { get; set; } = """";
    }
}";
        var template = @"---
subject: Hi {NonExistentProperty}!
---
@model MyApp.UserModel
<p>Hello @Model.MissingField</p>";

        var files = new[] { new TestAdditionalText("Templates/User.cshtml", template) };

        // Act
        var (diagnostics, _) = GeneratorTestRunner.Run(userCode, files);

        // Assert
        diagnostics.Should().Contain(d => d.Id == "TPL002" && d.GetMessage(null).Contains("MissingField"));
    }

    [Fact]
    public void Generator_TPL003_ShouldBeEmittedWhenLayoutOrPartialNotFound()
    {
        // Arrange
        var template = @"---
subject: Welcome
layout: _Layouts/MissingLayout
---
<Partial Name=""_Partials/MissingHeader"" />
<p>Body</p>";

        var files = new[] { new TestAdditionalText("Templates/Welcome.cshtml", template) };

        // Act
        var (diagnostics, _) = GeneratorTestRunner.Run(EmptyUserCode, files);

        // Assert
        diagnostics.Should().Contain(d => d.Id == "TPL003" && d.GetMessage(null).Contains("MissingLayout"));
        diagnostics.Should().Contain(d => d.Id == "TPL003" && d.GetMessage(null).Contains("MissingHeader"));
    }

    [Fact]
    public void Generator_TPL004_ShouldBeEmittedOnInvalidSampleJson()
    {
        // Arrange
        var files = new[]
        {
            new TestAdditionalText("Templates/Invoice.cshtml", "---\nsubject: Invoice\n---\n<p>Invoice</p>"),
            new TestAdditionalText("Templates/Invoice.sample.json", "INVALID JSON NOT STARTING WITH BRACKET")
        };

        // Act
        var (diagnostics, _) = GeneratorTestRunner.Run(EmptyUserCode, files);

        // Assert
        diagnostics.Should().Contain(d => d.Id == "TPL004");
    }

    [Fact]
    public void Generator_TPL005_ShouldBeEmittedWhenSubjectIsMissing()
    {
        // Arrange
        var template = @"---
layout: _Layouts/Base
---
<p>No subject here</p>";

        var files = new[]
        {
            new TestAdditionalText("Templates/_Layouts/Base.cshtml", "<html><body>@RenderBody()</body></html>"),
            new TestAdditionalText("Templates/NoSubject.cshtml", template)
        };

        // Act
        var (diagnostics, _) = GeneratorTestRunner.Run(EmptyUserCode, files);

        // Assert
        diagnostics.Should().Contain(d => d.Id == "TPL005");
    }

    [Fact]
    public void Generator_TPL006_ShouldBeEmittedOnDuplicateTemplateNames()
    {
        // Arrange
        var files = new[]
        {
            new TestAdditionalText("Templates/FolderA/Welcome.cshtml", "---\nsubject: A\n---\n<p>A</p>"),
            new TestAdditionalText("Templates/FolderB/Welcome.cshtml", "---\nsubject: B\n---\n<p>B</p>")
        };

        // Act
        var (diagnostics, _) = GeneratorTestRunner.Run(EmptyUserCode, files);

        // Assert
        diagnostics.Should().Contain(d => d.Id == "TPL006");
    }

    [Fact]
    public void Generator_TPL007_ShouldBeEmittedWhenTemplateExceeds90KB()
    {
        // Arrange
        var hugeBody = new string('A', 95 * 1024);
        var files = new[]
        {
            new TestAdditionalText("Templates/Huge.cshtml", $"---\nsubject: Huge\n---\n<p>{hugeBody}</p>")
        };

        // Act
        var (diagnostics, _) = GeneratorTestRunner.Run(EmptyUserCode, files);

        // Assert
        diagnostics.Should().Contain(d => d.Id == "TPL007");
    }

    [Fact]
    public void Generator_TPL008_ShouldBeEmittedOnUnsupportedSyntax()
    {
        // Arrange
        var template = @"---
subject: Functions
---
@functions {
    public void DoSomething() {}
}
<p>Test</p>";

        var files = new[] { new TestAdditionalText("Templates/Func.cshtml", template) };

        // Act
        var (diagnostics, _) = GeneratorTestRunner.Run(EmptyUserCode, files);

        // Assert
        diagnostics.Should().Contain(d => d.Id == "TPL008" && d.GetMessage(null).Contains("@functions"));
    }

    [Fact]
    public void Generator_TPL009_ShouldBeEmittedWhenLocaleKeyMissingInLocales()
    {
        // Arrange
        var template = @"---
subject: Localized
---
<p><T.Try Key=""nonexistent.key"" /></p>";

        var files = new[]
        {
            new TestAdditionalText("Templates/Loc.cshtml", template),
            new TestAdditionalText("Locales/en.json", "{\"other.key\": \"Hello\"}")
        };

        // Act
        var (diagnostics, _) = GeneratorTestRunner.Run(EmptyUserCode, files);

        // Assert
        diagnostics.Should().Contain(d => d.Id == "TPL009" && d.GetMessage(null).Contains("nonexistent.key"));
    }
}
