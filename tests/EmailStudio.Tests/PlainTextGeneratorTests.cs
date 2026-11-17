using EmailStudio.Text;
using FluentAssertions;
using Xunit;

namespace EmailStudio.Tests;

public sealed class PlainTextGeneratorTests
{
    [Fact]
    public void FromHtml_Links_ShouldFormatAsLabelAndUrl()
    {
        // Arrange
        var html = """<p>Please <a href="https://example.com/confirm">click here</a> to activate your account.</p>""";

        // Act
        var text = PlainTextGenerator.FromHtml(html);

        // Assert
        text.Should().Be("Please click here (https://example.com/confirm) to activate your account.");
    }

    [Fact]
    public void FromHtml_HeadingsAndListItems_ShouldFormatCleanly()
    {
        // Arrange
        var html = """
            <html>
            <head><style>.ignore { color: red; }</style></head>
            <body>
                <h1>Invoice #1042</h1>
                <p>Items purchased:</p>
                <ul>
                    <li>Subscription Pro &amp; Support</li>
                    <li>Setup Fee</li>
                </ul>
            </body>
            </html>
            """;

        // Act
        var text = PlainTextGenerator.FromHtml(html);

        // Assert
        text.Should().Contain("Invoice #1042");
        text.Should().Contain("Items purchased:");
        text.Should().Contain("• Subscription Pro & Support");
        text.Should().Contain("• Setup Fee");
        text.Should().NotContain("ignore");
        text.Should().NotContain("<h1");
    }
}

