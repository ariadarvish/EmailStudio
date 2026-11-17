using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using PreMailer.Net;

namespace EmailStudio.Inlining;

/// <summary>
/// Handles CSS inlining for email templates, ensuring style rules are moved
/// onto HTML element style attributes while preserving responsive @media blocks in the document head.
/// </summary>
public static partial class CssInliner
{
    private static readonly Regex StyleTagRegex = new(
        @"<style\b[^>]*>(.*?)</style>",
        RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);

    private static readonly Regex HeadTagRegex = new(
        @"</head>",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Inlines CSS rules from style blocks onto matching HTML elements while preserving media queries.
    /// </summary>
    /// <param name="html">The raw HTML content.</param>
    /// <param name="warnings">Collection to receive inlining warnings (e.g. pseudo-selectors).</param>
    /// <returns>HTML with CSS styles inlined onto element style attributes.</returns>
    public static string InlineCss(string html, IList<string>? warnings = null)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return string.Empty;
        }

        // 1. Extract any @media rules from <style> blocks before inlining
        var mediaRules = new List<string>();
        foreach (Match styleMatch in StyleTagRegex.Matches(html))
        {
            string cssContent = styleMatch.Groups[1].Value;
            var extractedMedia = ExtractMediaQueries(cssContent);
            mediaRules.AddRange(extractedMedia);
        }

        // 2. Perform CSS inlining via PreMailer
        var result = PreMailer.Net.PreMailer.MoveCssInline(
            html: html,
            removeStyleElements: true,
            ignoreElements: null,
            css: null,
            stripIdAndClassAttributes: false,
            removeComments: true);

        if (warnings != null && result.Warnings != null)
        {
            foreach (var warning in result.Warnings)
            {
                warnings.Add(warning);
            }
        }

        string inlinedHtml = result.Html;

        // 3. Re-inject preserved @media queries into <head>
        if (mediaRules.Count > 0)
        {
            var mediaStyleBlock = new StringBuilder();
            mediaStyleBlock.AppendLine();
            mediaStyleBlock.AppendLine("    <style type=\"text/css\">");
            foreach (var rule in mediaRules)
            {
                mediaStyleBlock.AppendLine($"        {rule.Trim()}");
            }
            mediaStyleBlock.AppendLine("    </style>");

            if (HeadTagRegex.IsMatch(inlinedHtml))
            {
                inlinedHtml = HeadTagRegex.Replace(inlinedHtml, $"{mediaStyleBlock.ToString().TrimEnd()}\n</head>", 1);
            }
            else
            {
                // If no <head>, insert at beginning of document
                inlinedHtml = $"<head>{mediaStyleBlock}</head>\n{inlinedHtml}";
            }
        }

        return inlinedHtml;
    }

    private static List<string> ExtractMediaQueries(string css)
    {
        var result = new List<string>();
        int index = 0;

        while (index < css.Length)
        {
            int mediaIndex = css.IndexOf("@media", index, StringComparison.OrdinalIgnoreCase);
            if (mediaIndex < 0)
            {
                break;
            }

            int openBrace = css.IndexOf('{', mediaIndex);
            if (openBrace < 0)
            {
                break;
            }

            int depth = 1;
            int current = openBrace + 1;
            while (current < css.Length && depth > 0)
            {
                if (css[current] == '{')
                {
                    depth++;
                }
                else if (css[current] == '}')
                {
                    depth--;
                }
                current++;
            }

            if (depth == 0)
            {
                string mediaBlock = css.Substring(mediaIndex, current - mediaIndex);
                result.Add(mediaBlock);
                index = current;
            }
            else
            {
                index = openBrace + 1;
            }
        }

        return result;
    }
}

