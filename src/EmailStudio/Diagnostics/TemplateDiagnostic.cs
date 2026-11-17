namespace EmailStudio;

/// <summary>
/// Severity level of an EmailStudio diagnostic message.
/// </summary>
public enum DiagnosticSeverityLevel
{
    Warning,
    Error
}

/// <summary>
/// Diagnostic report emitted during template parsing, rendering, or size validation.
/// </summary>
public sealed record TemplateDiagnostic(
    string Code,
    string Message,
    DiagnosticSeverityLevel Severity,
    string? FilePath = null,
    int? LineNumber = null);

/// <summary>
/// Base exception for EmailStudio errors carrying a diagnostic code.
/// </summary>
public class EmailStudioException : Exception
{
    public string DiagnosticCode { get; }
    public int? LineNumber { get; }

    public EmailStudioException(string diagnosticCode, string message, int? lineNumber = null, Exception? innerException = null)
        : base(message, innerException)
    {
        DiagnosticCode = diagnosticCode;
        LineNumber = lineNumber;
    }
}

/// <summary>
/// Thrown when template parsing fails (TPL001).
/// </summary>
public sealed class TemplateParseException : EmailStudioException
{
    public TemplateParseException(string message, int? lineNumber = null, Exception? innerException = null)
        : base("TPL001", message, lineNumber, innerException) { }
}

/// <summary>
/// Thrown when a referenced layout or partial template cannot be found (TPL003).
/// </summary>
public sealed class TemplateNotFoundException : EmailStudioException
{
    public TemplateNotFoundException(string templateType, string templateName)
        : base("TPL003", $"Referenced {templateType} '{templateName}' could not be found.") { }
}

/// <summary>
/// Thrown when template front matter is missing the required 'subject' property (TPL005).
/// </summary>
public sealed class MissingSubjectException : EmailStudioException
{
    public MissingSubjectException(string templateName)
        : base("TPL005", $"Template '{templateName}' front matter is missing the required 'subject' field.") { }
}

/// <summary>
/// Thrown when rendered HTML body exceeds the hard Gmail size threshold (TPL007).
/// </summary>
public sealed class EmailSizeExceededException : EmailStudioException
{
    public long ActualBytes { get; }
    public long MaxBytes { get; }

    public EmailSizeExceededException(string templateName, long actualBytes, long maxBytes)
        : base("TPL007", $"Rendered template '{templateName}' size ({actualBytes:N0} bytes) exceeds {maxBytes:N0} byte limit (Gmail clipping danger).")
    {
        ActualBytes = actualBytes;
        MaxBytes = maxBytes;
    }
}

/// <summary>
/// Thrown when unsupported Razor syntax is encountered in a template (TPL008).
/// </summary>
public sealed class UnsupportedSyntaxException : EmailStudioException
{
    public UnsupportedSyntaxException(string syntaxElement, int? lineNumber = null)
        : base("TPL008", $"Syntax element '{syntaxElement}' is not supported in EmailStudio templates.", lineNumber) { }
}

/// <summary>
/// Thrown when a required localization key is missing in strict mode (TPL009).
/// </summary>
public sealed class MissingLocaleKeyException : EmailStudioException
{
    public MissingLocaleKeyException(string key, string culture)
        : base("TPL009", $"Locale key '{key}' was not found in dictionary for culture '{culture}'.") { }
}
