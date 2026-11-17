using System.Collections.Generic;
using EmailStudio.TemplateEngine;
using FluentAssertions;
using Xunit;

namespace EmailStudio.Tests;

public sealed class TemplateEvaluatorTests
{
    private sealed record UserModel(string Name, string Email, bool IsActive, List<string> Roles);

    [Fact]
    public void Evaluate_PropertiesAndConditions_ShouldRenderCorrectly()
    {
        // Arrange
        var model = new UserModel("Aria", "aria@example.com", true, ["Admin", "Developer"]);
        var template = """
            <p>User: @Model.Name (@Model.Email)</p>
            @if (Model.IsActive) {
                <span>Status: Active</span>
            } else {
                <span>Status: Inactive</span>
            }
            """;

        // Act
        var result = TemplateEvaluator.Evaluate(template, model);

        // Assert
        result.Should().Contain("User: Aria (aria@example.com)");
        result.Should().Contain("Status: Active");
        result.Should().NotContain("Status: Inactive");
    }

    [Fact]
    public void Evaluate_ForeachLoop_ShouldIterateCollection()
    {
        // Arrange
        var model = new UserModel("Aria", "aria@example.com", true, ["Admin", "Editor"]);
        var template = """
            <ul>
            @foreach (var role in Model.Roles) {
                <li>@role</li>
            }
            </ul>
            """;

        // Act
        var result = TemplateEvaluator.Evaluate(template, model);

        // Assert
        result.Should().Contain("<li>Admin</li>");
        result.Should().Contain("<li>Editor</li>");
    }

    [Fact]
    public void Evaluate_Partials_ShouldExpandPartialContent()
    {
        // Arrange
        var model = new { CompanyName = "Acme Corp" };
        var template = """
            <p>Body</p>
            <Partial Name="_Partials/Footer" />
            """;

        // Act
        var result = TemplateEvaluator.Evaluate(
            template,
            model,
            partialResolver: name => name == "_Partials/Footer" ? "<footer>© 2026 @Model.CompanyName</footer>" : "");

        // Assert
        result.Should().Contain("<p>Body</p>");
        result.Should().Contain("<footer>© 2026 Acme Corp</footer>");
    }

    [Fact]
    public void ValidateSyntax_UnsupportedDirective_ShouldThrowTPL008()
    {
        // Arrange
        var template = """
            @functions {
                public int Add(int a, int b) => a + b;
            }
            <p>Content</p>
            """;

        // Act
        var act = () => TemplateEvaluator.ValidateSyntax(template);

        // Assert
        act.Should().Throw<UnsupportedSyntaxException>()
            .WithMessage("*@functions*")
            .Where(e => e.DiagnosticCode == "TPL008");
    }

    [Fact]
    public void InterpolateSubject_ShouldSubstitutePlaceholders()
    {
        // Arrange
        var model = new { Name = "Jane", OrderId = 98765 };
        var pattern = "Order #{OrderId} Confirmed for {Name}!";

        // Act
        var subject = TemplateEvaluator.InterpolateSubject(pattern, model);

        // Assert
        subject.Should().Be("Order #98765 Confirmed for Jane!");
    }
}
