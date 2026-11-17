using EmailStudio.Inlining;
using FluentAssertions;
using Xunit;

namespace EmailStudio.Tests;

public sealed class CssInlinerTests
{
    [Fact]
    public void InlineCss_ShouldMoveStylesToInlineAttributes()
    {
        // Arrange
        var html = """
            <html>
            <head>
                <style>
                    .brand { color: #2563eb; font-weight: bold; }
                    .card { background-color: #f3f4f6; padding: 20px; }
                </style>
            </head>
            <body>
                <div class="card">
                    <p class="brand">Welcome</p>
                </div>
            </body>
            </html>
            """;

        // Act
        var result = CssInliner.InlineCss(html);

        // Assert
        result.Should().Contain("color: #2563eb");
        result.Should().Contain("background-color: #f3f4f6");
    }

    [Fact]
    public void InlineCss_ShouldPreserveMediaQueriesInHead()
    {
        // Arrange
        var html = """
            <html>
            <head>
                <style>
                    .content { width: 600px; }
                    @media only screen and (max-width: 600px) {
                        .content { width: 100% !important; }
                    }
                </style>
            </head>
            <body>
                <div class="content">Responsive Box</div>
            </body>
            </html>
            """;

        // Act
        var result = CssInliner.InlineCss(html);

        // Assert
        result.Should().Contain("@media only screen and (max-width: 600px)");
        result.Should().Contain("width: 100% !important");
        result.Should().Contain("width: 600px");
    }
}
