using EmailStudio.Components;
using EmailStudio.I18n;
using FluentAssertions;
using Xunit;

namespace EmailStudio.Tests;

public sealed class ComponentExpanderTests
{
    [Fact]
    public void Expand_Button_ShouldRenderTableStructure()
    {
        // Arrange
        var input = """<Button Href="https://example.com/confirm" BgColor="#10b981">Verify Email</Button>""";

        // Act
        var result = ComponentExpander.Expand(input);

        // Assert
        result.Should().Contain("<table");
        result.Should().Contain("role=\"presentation\"");
        result.Should().Contain("href=\"https://example.com/confirm\"");
        result.Should().Contain("bgcolor=\"#10b981\"");
        result.Should().Contain("Verify Email");
    }

    [Fact]
    public void Expand_Heading_ShouldRenderCorrectHeadingTagAndStyles()
    {
        // Arrange
        var input = """<Heading Level="2" Color="#1e3a8a">Order Summary</Heading>""";

        // Act
        var result = ComponentExpander.Expand(input);

        // Assert
        result.Should().StartWith("<h2");
        result.Should().Contain("color:#1e3a8a");
        result.Should().Contain("Order Summary");
        result.Should().EndWith("</h2>");
    }

    [Fact]
    public void Expand_Divider_ShouldRenderNormalizedHr()
    {
        // Arrange
        var input = """<Divider Color="#cbd5e1" Margin="32px 0" />""";

        // Act
        var result = ComponentExpander.Expand(input);

        // Assert
        result.Should().Contain("<hr");
        result.Should().Contain("border-top:1px solid #cbd5e1");
        result.Should().Contain("margin:32px 0");
    }

    [Fact]
    public void Expand_Spacer_ShouldRenderOutlookSafeSpacer()
    {
        // Arrange
        var input = """<Spacer Height="28px" />""";

        // Act
        var result = ComponentExpander.Expand(input);

        // Assert
        result.Should().Contain("height:28px");
        result.Should().Contain("mso-line-height-rule:exactly");
        result.Should().Contain("&nbsp;");
    }

    [Fact]
    public void Expand_Image_ShouldRenderCleanImageTag()
    {
        // Arrange
        var input = """<Image Src="https://example.com/logo.png" Alt="Contoso Logo" Width="120" Height="40" />""";

        // Act
        var result = ComponentExpander.Expand(input);

        // Assert
        result.Should().Contain("<img");
        result.Should().Contain("src=\"https://example.com/logo.png\"");
        result.Should().Contain("alt=\"Contoso Logo\"");
        result.Should().Contain("width=\"120\"");
        result.Should().Contain("height=\"40\"");
    }

    [Fact]
    public void Expand_TTry_WithLocaleResolver_ShouldResolveTranslation()
    {
        // Arrange
        var localeManager = new LocaleManager();
        localeManager.AddTranslations("de-DE", new Dictionary<string, string>
        {
            ["footer.note"] = "Bitte antworte nicht auf diese E-Mail."
        });

        var input = """<p><T.Try("footer.note") /></p>""";

        // Act
        var result = ComponentExpander.Expand(input, localeManager, culture: "de-DE");

        // Assert
        result.Should().Be("<p>Bitte antworte nicht auf diese E-Mail.</p>");
    }

    [Fact]
    public void Expand_TTry_Fallback_ShouldFallbackToBaseCulture()
    {
        // Arrange
        var localeManager = new LocaleManager();
        localeManager.AddTranslations("de", new Dictionary<string, string>
        {
            ["common.greeting"] = "Guten Tag"
        });

        var input = """<p><T.Try Key="common.greeting" /></p>""";

        // Act
        var result = ComponentExpander.Expand(input, localeManager, culture: "de-AT");

        // Assert
        result.Should().Be("<p>Guten Tag</p>");
    }

    [Fact]
    public void Expand_TTry_StrictModeMissingKey_ShouldThrowTPL009()
    {
        // Arrange
        var localeManager = new LocaleManager();
        var input = """<T.Try("missing.key") />""";

        // Act
        var act = () => ComponentExpander.Expand(input, localeManager, culture: "de-DE", strictMode: true);

        // Assert
        act.Should().Throw<MissingLocaleKeyException>()
            .Where(e => e.DiagnosticCode == "TPL009");
    }
}

