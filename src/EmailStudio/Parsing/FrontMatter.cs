namespace EmailStudio.Parsing;

/// <summary>
/// Parsed YAML front matter metadata.
/// </summary>
public sealed record FrontMatter(
    string? Subject,
    string? Layout,
    string? PlainText,
    IReadOnlyDictionary<string, string> CustomProperties)
{
    public static readonly FrontMatter Empty = new(null, null, null, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));
}

/// <summary>
/// Represents a parsed template document consisting of front matter and body content.
/// </summary>
public sealed record TemplateDocument(
    FrontMatter FrontMatter,
    string Body,
    string? ModelType = null);

