using System;
using System.Text;

namespace EmailStudio.SizeGuard;

/// <summary>
/// Validates rendered email HTML byte size against Gmail clipping thresholds.
/// </summary>
public static class EmailSizeGuard
{
    /// <summary>
    /// Default warning threshold: 90 KB (92,160 bytes).
    /// </summary>
    public const long DefaultWarningThresholdBytes = 90 * 1024;

    /// <summary>
    /// Default hard limit threshold: 102 KB (104,448 bytes) — Gmail clipping limit.
    /// </summary>
    public const long DefaultMaxThresholdBytes = 102 * 1024;

    /// <summary>
    /// Validates the HTML byte size against warning and maximum thresholds.
    /// </summary>
    /// <param name="html">Rendered HTML string.</param>
    /// <param name="templateName">Name of the template being validated.</param>
    /// <param name="warningThresholdBytes">Warning limit in bytes.</param>
    /// <param name="maxThresholdBytes">Hard error limit in bytes.</param>
    /// <param name="onWarning">Optional callback when warning threshold is exceeded.</param>
    /// <returns>The calculated byte length in UTF-8.</returns>
    /// <exception cref="EmailSizeExceededException">Thrown when byte size exceeds the maximum threshold.</exception>
    public static long Validate(
        string html,
        string templateName,
        long warningThresholdBytes = DefaultWarningThresholdBytes,
        long maxThresholdBytes = DefaultMaxThresholdBytes,
        Action<TemplateDiagnostic>? onWarning = null)
    {
        ArgumentNullException.ThrowIfNull(html);

        long byteCount = Encoding.UTF8.GetByteCount(html);

        if (byteCount > maxThresholdBytes)
        {
            throw new EmailSizeExceededException(templateName, byteCount, maxThresholdBytes);
        }

        if (byteCount > warningThresholdBytes && onWarning != null)
        {
            var diagnostic = new TemplateDiagnostic(
                Code: "TPL007",
                Message: $"Rendered template '{templateName}' size ({byteCount:N0} bytes) exceeds warning threshold {warningThresholdBytes:N0} bytes (Gmail clipping danger).",
                Severity: DiagnosticSeverityLevel.Warning);

            onWarning(diagnostic);
        }

        return byteCount;
    }
}
