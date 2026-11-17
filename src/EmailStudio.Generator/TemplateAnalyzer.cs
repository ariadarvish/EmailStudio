using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Text.RegularExpressions;

namespace EmailStudio.Generator;

/// <summary>
/// Analyzes .cshtml template files for the Roslyn source generator.
/// </summary>
public static class TemplateAnalyzer
{
    private static readonly string[] ForbiddenDirectives =
    {
        "@functions",
        "@await",
        "@section",
        "@inject",
        "@inherits",
        "@implements"
    };

    private static readonly Regex ModelRegex = new Regex(
        @"^@model\s+([A-Za-z0-9_.]+)",
        RegexOptions.Compiled | RegexOptions.Multiline);

    private static readonly Regex ModelPropRegex = new Regex(
        @"@Model\.([a-zA-Z0-9_]+)",
        RegexOptions.Compiled);

    private static readonly Regex SubjectPlaceholderRegex = new Regex(
        @"\{([a-zA-Z0-9_.]+)\}",
        RegexOptions.Compiled);

    private static readonly Regex PartialRegex = new Regex(
        @"<(?:Partial|Include)\s+(?:Name|Path)=""([^""]+)""\s*/?>",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex TTryTagRegex = new Regex(
        @"<T\.Try\s+Key=""([^""]+)""\s*/?>",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex TTryMethodRegex = new Regex(
        @"<T\.Try\(\""([^\""]+)\""\)\s*/?>",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static TemplateFileInfo Analyze(string filePath, string content)
    {
        string fileName = Path.GetFileName(filePath);
        string templateName = Path.GetFileNameWithoutExtension(filePath);

        // Normalize template name (e.g. Welcome -> Welcome)
        if (templateName.StartsWith("_"))
        {
            templateName = templateName.Substring(1);
        }

        bool isLayout = filePath.IndexOf("_Layouts", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        fileName.IndexOf("Layout", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        content.IndexOf("@RenderBody()", StringComparison.OrdinalIgnoreCase) >= 0;

        bool isPartial = !isLayout && (fileName.StartsWith("_") ||
                         filePath.IndexOf("_Partials", StringComparison.OrdinalIgnoreCase) >= 0);

        string? subject = null;
        string? layout = null;
        string? plainText = null;
        string? parseError = null;
        int? parseErrorLine = null;
        string body = content;

        // Strip BOM if present
        if (content.Length > 0 && content[0] == '\uFEFF')
        {
            content = content.Substring(1);
        }

        string trimmed = content.TrimStart();
        if (trimmed.StartsWith("---"))
        {
            using (var reader = new StringReader(trimmed))
            {
                string firstLine = reader.ReadLine();
                if (firstLine != null && firstLine.Trim() == "---")
                {
                    var lines = new List<string>();
                    string line;
                    bool closed = false;
                    int lineNum = 1;

                    while ((line = reader.ReadLine()) != null)
                    {
                        lineNum++;
                        if (line.Trim() == "---")
                        {
                            closed = true;
                            break;
                        }
                        lines.Add(line);
                    }

                    if (!closed)
                    {
                        parseError = "Unclosed front matter block. Missing terminating '---'.";
                        parseErrorLine = lineNum;
                    }
                    else
                    {
                        foreach (var yLine in lines)
                        {
                            int colon = yLine.IndexOf(':');
                            if (colon > 0)
                            {
                                string k = yLine.Substring(0, colon).Trim().ToLowerInvariant();
                                string v = yLine.Substring(colon + 1).Trim().Trim('"', '\'');
                                if (k == "subject") subject = v;
                                else if (k == "layout") layout = v;
                                else if (k == "plaintext") plainText = v;
                            }
                        }
                        body = reader.ReadToEnd() ?? string.Empty;
                    }
                }
            }
        }

        // Extract @model
        string? modelType = null;
        var modelMatch = ModelRegex.Match(body);
        if (modelMatch.Success)
        {
            modelType = modelMatch.Groups[1].Value.Trim();
        }

        // Check unsupported syntax
        string? unsupportedSyntax = null;
        int? unsupportedSyntaxLine = null;
        foreach (var directive in ForbiddenDirectives)
        {
            int idx = body.IndexOf(directive, StringComparison.OrdinalIgnoreCase);
            if (idx >= 0)
            {
                unsupportedSyntax = directive;
                int line = 1;
                for (int i = 0; i < idx; i++)
                {
                    if (body[i] == '\n') line++;
                }
                unsupportedSyntaxLine = line;
                break;
            }
        }

        // Collect referenced properties
        var properties = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (Match match in ModelPropRegex.Matches(body))
        {
            properties.Add(match.Groups[1].Value);
        }
        if (!string.IsNullOrEmpty(subject))
        {
            foreach (Match match in SubjectPlaceholderRegex.Matches(subject))
            {
                string prop = match.Groups[1].Value;
                if (prop.StartsWith("Model.", StringComparison.OrdinalIgnoreCase))
                {
                    prop = prop.Substring(6);
                }
                int dotIdx = prop.IndexOf('.');
                if (dotIdx > 0)
                {
                    prop = prop.Substring(0, dotIdx);
                }
                properties.Add(prop);
            }
        }

        // Collect referenced partials
        var partials = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (Match match in PartialRegex.Matches(body))
        {
            partials.Add(match.Groups[1].Value);
        }

        // Collect referenced locale keys
        var localeKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (Match match in TTryTagRegex.Matches(body))
        {
            localeKeys.Add(match.Groups[1].Value);
        }
        foreach (Match match in TTryMethodRegex.Matches(body))
        {
            localeKeys.Add(match.Groups[1].Value);
        }

        return new TemplateFileInfo(
            filePath: filePath,
            fileName: fileName,
            templateName: templateName,
            isLayout: isLayout,
            isPartial: isPartial,
            content: content,
            subject: subject,
            layout: layout,
            modelType: modelType,
            plainText: plainText,
            referencedProperties: properties.ToImmutableArray(),
            referencedPartials: partials.ToImmutableArray(),
            referencedLocaleKeys: localeKeys.ToImmutableArray(),
            parseError: parseError,
            parseErrorLine: parseErrorLine,
            unsupportedSyntax: unsupportedSyntax,
            unsupportedSyntaxLine: unsupportedSyntaxLine);
    }
}

