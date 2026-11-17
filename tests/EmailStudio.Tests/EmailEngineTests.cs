using System;
using EmailStudio;
using FluentAssertions;
using Xunit;

namespace EmailStudio.Tests;

public sealed class EmailEngineTests
{
    private sealed record WelcomeModel(string Name, string ActionUrl, string CompanyName);

    [Fact]
    public void Render_EndToEnd_ShouldProduceCompleteEmailDraft()
    {
        // Arrange
        var template = """
            ---
            subject: Welcome to {CompanyName}, {Name}!
            layout: _Layouts/Base
            ---
            @model EmailStudio.Tests.WelcomeModel

            <Heading Level="1">Welcome aboard, @Model.Name!</Heading>
            <p class="lead">Thanks for joining us at @Model.CompanyName.</p>
            <Button Href="@Model.ActionUrl" BgColor="#2563eb">Activate Account</Button>
            <Spacer Height="24" />
            <Divider />
            <p class="muted">Need help? Contact support.</p>
            """;

        var baseLayout = """
            <!DOCTYPE html>
            <html>
            <head>
                <style>
                    .lead { font-size: 16px; color: #374151; }
                    .muted { font-size: 12px; color: #9ca3af; }
                </style>
            </head>
            <body style="margin: 0; padding: 20px; background-color: #f9fafb;">
                <div style="max-width: 600px; margin: 0 auto; background: #ffffff; padding: 32px;">
                    @RenderBody()
                </div>
            </body>
            </html>
            """;

        var model = new WelcomeModel("Alex", "https://example.com/activate?token=abc", "Acme Inc");

        var options = new EmailRenderOptions
        {
            LayoutResolver = name => name == "_Layouts/Base" ? baseLayout : null
        };

        // Act
        var draft = EmailEngine.Render(template, model, options, "Welcome");

        // Assert: Subject
        draft.Subject.Should().Be("Welcome to Acme Inc, Alex!");

        // Assert: Inlined HTML Body
        draft.HtmlBody.Should().Contain("Welcome aboard, Alex!");
        draft.HtmlBody.Should().Contain("color: #374151"); // inlined from .lead
        draft.HtmlBody.Should().Contain("color: #9ca3af"); // inlined from .muted
        draft.HtmlBody.Should().Contain("href=\"https://example.com/activate?token=abc\"");
        draft.HtmlBody.Should().Contain("role=\"presentation\""); // Table button

        // Assert: Plain Text Body
        draft.TextBody.Should().Contain("Welcome aboard, Alex!");
        draft.TextBody.Should().Contain("Thanks for joining us at Acme Inc.");
        draft.TextBody.Should().Contain("Activate Account (https://example.com/activate?token=abc)");
        draft.TextBody.Should().Contain("Need help? Contact support.");
    }

    [Fact]
    public void Render_DeterministicOutput_ShouldProduceByteIdenticalResultsAcrossRuns()
    {
        // Arrange
        var template = """
            ---
            subject: Notification for {Name}
            ---
            <h1>Hello @Model.Name</h1>
            <p>Your balance is @Model.Balance.</p>
            <Button Href="https://example.com">View Balance</Button>
            """;

        var model = new { Name = "Sam", Balance = "$500" };

        // Act
        var run1 = EmailEngine.Render(template, model);
        var run2 = EmailEngine.Render(template, model);

        // Assert: Byte-for-byte identical
        run1.Subject.Should().Be(run2.Subject);
        run1.HtmlBody.Should().Be(run2.HtmlBody);
        run1.TextBody.Should().Be(run2.TextBody);

        var bytes1 = System.Text.Encoding.UTF8.GetBytes(run1.HtmlBody);
        var bytes2 = System.Text.Encoding.UTF8.GetBytes(run2.HtmlBody);
        bytes1.Should().Equal(bytes2);
    }

    [Fact]
    public void Render_MissingSubject_ShouldThrowTPL005()
    {
        // Arrange
        var template = """
            ---
            layout: _Layouts/Base
            ---
            <p>No subject here</p>
            """;

        // Act
        var act = () => EmailEngine.Render(template, new { });

        // Assert
        act.Should().Throw<MissingSubjectException>()
            .Where(e => e.DiagnosticCode == "TPL005");
    }

    [Fact]
    public void Render_MissingLayout_ShouldThrowTPL003()
    {
        // Arrange
        var template = """
            ---
            subject: My Subject
            layout: _Layouts/NonExistent
            ---
            <p>Content</p>
            """;

        // Act
        var act = () => EmailEngine.Render(template, new { }, new EmailRenderOptions { LayoutResolver = _ => null });

        // Assert
        act.Should().Throw<TemplateNotFoundException>()
            .Where(e => e.DiagnosticCode == "TPL003");
    }
}
