using System;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace EmailStudio.Text;

/// <summary>
/// Converts rendered HTML into a clean, human-readable plain text email alternative.
/// </summary>
public static partial class PlainTextGenerator
{
    private static readonly Regex HeadStyleScriptRegex = new(
        @"<(head|style|script)\b[^>]*>.*?</\1>",
        RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);

    private static readonly Regex LinkRegex = new(
        @"<a\b[^>]*\bhref=[""']([^""']+)[""'][^>]*>(.*?)</a>",
        RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);

    private static readonly Regex HeadingRegex = new(
        @"<h([1-6])\b[^>]*>(.*?)</h\1>",
        RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);

    private static readonly Regex ListItemRegex = new(
        @"<li\b[^>]*>(.*?)</li>",
        RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);

    private static readonly Regex BreakRegex = new(
        @"<(br|hr)\b[^>]*>",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex BlockRegex = new(
        @"</?(p|div|tr|table|ul|ol|blockquote)\b[^>]*>",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex RemainingTagsRegex = new(
        @"<[^>]+>",
        RegexOptions.Compiled);

    private static readonly Regex MultipleNewlinesRegex = new(
        @"(\r?\n\s*){3,}",
        RegexOptions.Compiled);

    /// <summary>
    /// Generates plain text representation from HTML content.
    /// </summary>
    public static string FromHtml(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return string.Empty;
        }

        // 1. Remove head, style, and script blocks
        string text = HeadStyleScriptRegex.Replace(html, string.Empty);

        // 2. Convert links: <a href="url">Label</a> -> Label (url)
        text = LinkRegex.Replace(text, match =>
        {
            string url = match.Groups[1].Value.Trim();
            string label = RemainingTagsRegex.Replace(match.Groups[2].Value, "").Trim();

            if (string.IsNullOrEmpty(label) || string.Equals(label, url, StringComparison.OrdinalIgnoreCase))
            {
                return url;
            }

            return $"{label} ({url})";
        });

        // 3. Convert headings to formatted text blocks
        text = HeadingRegex.Replace(text, match =>
        {
            string headingText = RemainingTagsRegex.Replace(match.Groups[2].Value, "").Trim();
            return $"\n\n{headingText}\n\n";
        });

        // 4. Convert list items to bullet points
        text = ListItemRegex.Replace(text, match =>
        {
            string itemText = RemainingTagsRegex.Replace(match.Groups[1].Value, "").Trim();
            return $"\n• {itemText}";
        });

        // 5. Line breaks and block tags
        text = BreakRegex.Replace(text, "\n");
        text = BlockRegex.Replace(text, "\n");

        // 6. Strip any remaining tags
        text = RemainingTagsRegex.Replace(text, string.Empty);

        // 7. Decode HTML entities
        text = WebUtility.HtmlDecode(text);

        // Replace non-breaking spaces with standard space
        text = text.Replace('\u00A0', ' ');

        // 8. Normalize whitespace and consecutive empty lines
        string[] lines = text.Split(['\r', '\n'], StringSplitOptions.None);
        var sb = new StringBuilder();
        bool lastWasEmpty = false;

        foreach (string rawLine in lines)
        {
            string trimmed = rawLine.Trim();
            if (string.IsNullOrEmpty(trimmed))
            {
                if (!lastWasEmpty)
                {
                    sb.AppendLine();
                    lastWasEmpty = true;
                }
            }
            else
            {
                sb.AppendLine(trimmed);
                lastWasEmpty = false;
            }
        }

        return sb.ToString().Trim();
    }
}
