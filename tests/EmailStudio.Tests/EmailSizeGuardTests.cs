using EmailStudio;
using EmailStudio.SizeGuard;
using FluentAssertions;
using Xunit;

namespace EmailStudio.Tests;

public sealed class EmailSizeGuardTests
{
    [Fact]
    public void Validate_WithinNormalLimits_ShouldPassSilently()
    {
        // Arrange
        var html = "<html><body><p>Normal size email</p></body></html>";
        TemplateDiagnostic? warning = null;

        // Act
        long bytes = EmailSizeGuard.Validate(html, "Test.cshtml", onWarning: d => warning = d);

        // Assert
        bytes.Should().BeGreaterThan(0);
        warning.Should().BeNull();
    }

    [Fact]
    public void Validate_BetweenWarningAndMax_ShouldTriggerWarningCallback()
    {
        // Arrange: Generate string larger than 90 KB but under 102 KB (e.g. 95 KB)
        var html = new string('A', 95 * 1024);
        TemplateDiagnostic? warning = null;

        // Act
        long bytes = EmailSizeGuard.Validate(html, "Warning.cshtml", onWarning: d => warning = d);

        // Assert
        bytes.Should().Be(95 * 1024);
        warning.Should().NotBeNull();
        warning!.Code.Should().Be("TPL007");
        warning.Severity.Should().Be(DiagnosticSeverityLevel.Warning);
    }

    [Fact]
    public void Validate_ExceedingMaxThreshold_ShouldThrowEmailSizeExceededException()
    {
        // Arrange: Generate string larger than 102 KB (e.g. 105 KB)
        var html = new string('X', 105 * 1024);

        // Act
        var act = () => EmailSizeGuard.Validate(html, "TooLarge.cshtml");

        // Assert
        act.Should().Throw<EmailSizeExceededException>()
            .WithMessage("*exceeds*")
            .Where(e => e.DiagnosticCode == "TPL007" && e.ActualBytes == 105 * 1024);
    }
}
