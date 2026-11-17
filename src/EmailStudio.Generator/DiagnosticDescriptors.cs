using Microsoft.CodeAnalysis;

namespace EmailStudio.Generator;

/// <summary>
/// Stable diagnostic descriptors for EmailStudio template compilation and validation.
/// </summary>
public static class DiagnosticDescriptors
{
    private const string Category = "EmailStudio";

    public static readonly DiagnosticDescriptor TPL001_ParseError = new(
        id: "TPL001",
        title: "Template file cannot be parsed",
        messageFormat: "Template file '{0}' cannot be parsed: {1}",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor TPL002_UnknownModelProperty = new(
        id: "TPL002",
        title: "Unknown property on declared model type",
        messageFormat: "Property '{0}' does not exist on model type '{1}'",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor TPL003_LayoutOrPartialNotFound = new(
        id: "TPL003",
        title: "Referenced layout or partial not found",
        messageFormat: "Referenced {0} '{1}' could not be found",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor TPL004_InvalidSampleJson = new(
        id: "TPL004",
        title: "Missing or invalid sample JSON",
        messageFormat: "Sample JSON file '{0}' is missing or contains invalid JSON: {1}",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor TPL005_MissingSubject = new(
        id: "TPL005",
        title: "Missing subject in front matter",
        messageFormat: "Template '{0}' front matter is missing the required 'subject' field",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor TPL006_DuplicateTemplateName = new(
        id: "TPL006",
        title: "Duplicate template name",
        messageFormat: "Duplicate template name '{0}' detected at '{1}'",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor TPL007_HtmlSizeExceeded = new(
        id: "TPL007",
        title: "Rendered HTML exceeds size threshold",
        messageFormat: "Rendered template '{0}' size ({1:N0} bytes) exceeds {2:N0} byte limit (Gmail clipping danger)",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor TPL008_UnsupportedSyntax = new(
        id: "TPL008",
        title: "Unsupported syntax element",
        messageFormat: "Syntax element '{0}' is not supported in EmailStudio templates",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor TPL009_MissingLocaleKey = new(
        id: "TPL009",
        title: "Missing locale key (strict mode)",
        messageFormat: "Locale key '{0}' was not found in dictionary '{1}'",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);
}

