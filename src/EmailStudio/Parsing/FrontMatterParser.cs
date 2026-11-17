using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace EmailStudio.Parsing;

/// <summary>
/// Parser for YAML front matter and template document structure.
/// </summary>
public static partial class FrontMatterParser
{
    private static readonly Regex ModelDirectiveRegex = new(
        @"^@model\s+([A-Za-z0-9_.]+)\s*(\r?\n|$)",
        RegexOptions.Compiled | RegexOptions.Multiline);

    /// <summary>
    /// Parses template content into front matter metadata and body content.
    /// </summary>
    /// <param name="content">Raw template file content.</param>
    /// <param name="templateName">Name or path of template for diagnostic reporting.</param>
    /// <returns>A <see cref="TemplateDocument"/> containing parsed front matter and remaining body.</returns>
    public static TemplateDocument Parse(string content, string templateName = "Template")
    {
        ArgumentNullException.ThrowIfNull(content);

        // Strip UTF-8 byte order mark if present
        if (content.StartsWith("\uFEFF", StringComparison.Ordinal))
        {
            content = content[1..];
        }

        string trimmed = content.TrimStart();
        FrontMatter frontMatter = FrontMatter.Empty;
        string body = content;

        if (trimmed.StartsWith("---", StringComparison.Ordinal))
        {
            using var reader = new StringReader(trimmed);
            string? firstLine = reader.ReadLine(); // "---"

            if (firstLine != null && firstLine.Trim() == "---")
            {
                var yamlLines = new List<string>();
                string? line;
                bool closingFound = false;
                int currentLineNumber = 1;

                while ((line = reader.ReadLine()) != null)
                {
                    currentLineNumber++;
                    if (line.Trim() == "---")
                    {
                        closingFound = true;
                        break;
                    }
                    yamlLines.Add(line);
                }

                if (!closingFound)
                {
                    throw new TemplateParseException(
                        $"Unclosed front matter block in template '{templateName}'. Missing terminating '---'.",
                        currentLineNumber);
                }

                frontMatter = ParseYamlLines(yamlLines, templateName);
                body = reader.ReadToEnd().TrimStart('\r', '\n');
            }
        }

        // Extract and strip @model directive if present at the top of the body
        string? modelType = null;
        var modelMatch = ModelDirectiveRegex.Match(body);
        if (modelMatch.Success)
        {
            modelType = modelMatch.Groups[1].Value.Trim();
            body = body.Remove(modelMatch.Index, modelMatch.Length).TrimStart('\r', '\n');
        }

        return new TemplateDocument(frontMatter, body, modelType);
    }

    private static FrontMatter ParseYamlLines(List<string> lines, string templateName)
    {
        string? subject = null;
        string? layout = null;
        string? plainText = null;
        var custom = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        string? currentKey = null;
        var multilineBuffer = new List<string>();

        void FlushCurrentKey()
        {
            if (currentKey == null)
            {
                return;
            }

            string combinedValue = string.Join("\n", multilineBuffer).Trim();
            multilineBuffer.Clear();

            switch (currentKey.ToLowerInvariant())
            {
                case "subject":
                    subject = combinedValue;
                    break;
                case "layout":
                    layout = combinedValue;
                    break;
                case "plaintext":
                    plainText = combinedValue;
                    break;
                default:
                    custom[currentKey] = combinedValue;
                    break;
            }
            currentKey = null;
        }

        for (int i = 0; i < lines.Count; i++)
        {
            string line = lines[i];
            if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith('#'))
            {
                continue;
            }

            int colonIndex = line.IndexOf(':');
            if (colonIndex > 0 && !line.StartsWith(" ", StringComparison.Ordinal) && !line.StartsWith("\t", StringComparison.Ordinal))
            {
                FlushCurrentKey();
                string key = line[..colonIndex].Trim();
                string val = line[(colonIndex + 1)..].Trim();

                // Unquote if wrapped in single or double quotes
                if ((val.StartsWith('"') && val.EndsWith('"') && val.Length >= 2) ||
                    (val.StartsWith('\'') && val.EndsWith('\'') && val.Length >= 2))
                {
                    val = val[1..^1];
                }

                currentKey = key;
                if (!string.IsNullOrEmpty(val))
                {
                    multilineBuffer.Add(val);
                }
            }
            else if (currentKey != null)
            {
                // Indented line continuation
                multilineBuffer.Add(line.Trim());
            }
            else
            {
                throw new TemplateParseException($"Invalid YAML syntax at line {i + 2} in '{templateName}': {line}", i + 2);
            }
        }

        FlushCurrentKey();
        return new FrontMatter(subject, layout, plainText, custom);
    }
}
