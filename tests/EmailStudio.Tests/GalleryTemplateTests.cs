using System;
using System.IO;
using System.Linq;
using EmailStudio.Preview;
using FluentAssertions;
using Xunit;

namespace EmailStudio.Tests;

public sealed class GalleryTemplateTests
{
    private readonly string _galleryTemplatesDir;
    private readonly string _galleryLocalesDir;

    public GalleryTemplateTests()
    {
        // Resolve path to samples/Gallery
        string currentDir = AppContext.BaseDirectory;
        string? projectRoot = currentDir;
        while (projectRoot != null && !File.Exists(Path.Combine(projectRoot, "EmailStudio.slnx")))
        {
            projectRoot = Path.GetDirectoryName(projectRoot);
        }

        projectRoot.Should().NotBeNull("Must find repository root containing EmailStudio.slnx");
        _galleryTemplatesDir = Path.Combine(projectRoot!, "samples", "Gallery", "Templates");
        _galleryLocalesDir = Path.Combine(projectRoot!, "samples", "Gallery", "Locales");
    }

    [Fact]
    public void Gallery_RichTemplates_ShouldRenderSuccessfullyWithinSizeLimits()
    {
        // Arrange
        var options = new PreviewServerOptions
        {
            TemplateDirectory = _galleryTemplatesDir,
            LocalesDirectory = _galleryLocalesDir
        };
        var service = new TemplateService(options);
        var templates = service.GetTemplates();

        // Assert: 5 rich production templates present
        templates.Should().HaveCount(5);

        string[] expectedTemplateNames =
        [
            "ProductLaunch",
            "Welcome",
            "InvoiceReceipt",
            "ShippingNotification",
            "SecurityAlert"
        ];

        templates.Select(t => t.Name).Should().BeEquivalentTo(expectedTemplateNames);

        // Act & Assert each template
        foreach (var t in templates)
        {
            var result = service.Render(t.Name);

            result.Success.Should().BeTrue($"Template '{t.Name}' should render without errors, but had: {result.Diagnostic?.Message}");
            result.Draft.Should().NotBeNull();
            result.Draft!.Subject.Should().NotBeNullOrWhiteSpace();
            result.Draft.Subject.Should().NotContain("{", "Subject placeholders should be fully resolved");
            result.Draft.HtmlBody.Should().NotBeNullOrWhiteSpace();
            result.Draft.TextBody.Should().NotBeNullOrWhiteSpace();

            int byteLength = System.Text.Encoding.UTF8.GetByteCount(result.Draft.HtmlBody);
            byteLength.Should().BeLessThan(102 * 1024, $"Template '{t.Name}' must not exceed 102 KB Gmail clipping limit");
        }
    }

    [Fact]
    public void Gallery_Templates_ShouldRenderDeterministicallyAcrossRuns()
    {
        // Arrange
        var options = new PreviewServerOptions
        {
            TemplateDirectory = _galleryTemplatesDir,
            LocalesDirectory = _galleryLocalesDir
        };
        var service = new TemplateService(options);

        // Act
        var run1 = service.Render("Welcome");
        var run2 = service.Render("Welcome");

        // Assert
        run1.Draft!.HtmlBody.Should().Be(run2.Draft!.HtmlBody);
        run1.Draft.TextBody.Should().Be(run2.Draft.TextBody);
        run1.Draft.Subject.Should().Be(run2.Draft.Subject);
    }

    [Fact]
    public void Gallery_Localization_ShouldResolveGermanAndFrenchLocales()
    {
        // Arrange
        var options = new PreviewServerOptions
        {
            TemplateDirectory = _galleryTemplatesDir,
            LocalesDirectory = _galleryLocalesDir
        };
        var service = new TemplateService(options);

        // Act
        var deResult = service.Render("Welcome", "de-DE");
        var frResult = service.Render("Welcome", "fr-FR");

        // Assert
        deResult.Success.Should().BeTrue();
        frResult.Success.Should().BeTrue();

        deResult.Draft!.HtmlBody.Should().Contain("Bitte antworten Sie nicht");
        frResult.Draft!.HtmlBody.Should().Contain("Veuillez ne pas répondre");
    }

    [Fact]
    public void Gallery_ProductLaunch_ShouldRenderImagesWithEmailSafeAttributes()
    {
        // Arrange
        var options = new PreviewServerOptions
        {
            TemplateDirectory = _galleryTemplatesDir,
            LocalesDirectory = _galleryLocalesDir
        };
        var service = new TemplateService(options);

        // Act
        var result = service.Render("ProductLaunch");

        // Assert
        result.Success.Should().BeTrue();
        result.Draft!.HtmlBody.Should().Contain("https://dummyimage.com/1072x480/0f172a/38bdf8.png");
        result.Draft.HtmlBody.Should().Contain("width=\"536\" height=\"240\"");
        result.Draft.HtmlBody.Should().Contain("display: block;outline: none;border: none;text-decoration: none;max-width: 100%");
        result.Draft.HtmlBody.Should().Contain("https://dummyimage.com/96x96/64748b/ffffff.png");
    }
}

