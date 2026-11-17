using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using EmailStudio.I18n;

namespace EmailStudio.Components;

/// <summary>
/// Expands EmailStudio high-level components into email-safe, table-based HTML markup.
/// </summary>
public static partial class ComponentExpander
{
    private static readonly Regex ButtonRegex = new(
        @"<Button\b([^>]*)>(.*?)</Button>",
        RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);

    private static readonly Regex HeadingRegex = new(
        @"<Heading\b([^>]*)>(.*?)</Heading>",
        RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);

    private static readonly Regex DividerRegex = new(
        @"<Divider\b([^>]*)/?>",
        RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);

    private static readonly Regex SpacerRegex = new(
        @"<Spacer\b([^>]*)/?>",
        RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);

    private static readonly Regex ImageRegex = new(
        @"<Image\b([^>]*)/?>",
        RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);

    private static readonly Regex TTryTagRegex = new(
        @"<T\.Try\s+Key=""([^""]+)""\s*/?>",
        RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);

    private static readonly Regex TTryMethodRegex = new(
        @"<T\.Try\(\""([^\""]+)\""\)\s*/?>",
        RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);

    /// <summary>
    /// Expands all EmailStudio components within the provided HTML markup.
    /// </summary>
    public static string Expand(string html, ILocaleResolver? localeResolver = null, string? culture = null, bool strictMode = false)
    {
        if (string.IsNullOrEmpty(html))
        {
            return string.Empty;
        }

        // 1. Expand <Button>
        html = ButtonRegex.Replace(html, match =>
        {
            var attrs = ParseAttributes(match.Groups[1].Value);
            string innerText = match.Groups[2].Value;

            string href = attrs.GetValueOrDefault("href", "#");
            string align = attrs.GetValueOrDefault("align", "center");
            string bgColor = attrs.GetValueOrDefault("bgcolor", "#2563eb");
            string textColor = attrs.GetValueOrDefault("textcolor", "#ffffff");
            string borderRadius = attrs.GetValueOrDefault("borderradius", "4px");
            string padding = attrs.GetValueOrDefault("padding", "12px 24px");
            string target = attrs.GetValueOrDefault("target", "_blank");

            return $"""<table border="0" cellpadding="0" cellspacing="0" role="presentation" style="margin:0 auto;text-align:{align};"><tr><td align="{align}" bgcolor="{bgColor}" style="border-radius:{borderRadius};background-color:{bgColor};"><a href="{href}" target="{target}" style="display:inline-block;padding:{padding};font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif;font-size:14px;font-weight:600;color:{textColor};text-decoration:none;border-radius:{borderRadius};">{innerText}</a></td></tr></table>""";
        });

        // 2. Expand <Heading>
        html = HeadingRegex.Replace(html, match =>
        {
            var attrs = ParseAttributes(match.Groups[1].Value);
            string innerText = match.Groups[2].Value;

            string level = attrs.GetValueOrDefault("level", "1");
            string color = attrs.GetValueOrDefault("color", "#111827");
            string align = attrs.GetValueOrDefault("align", "left");

            var (fontSize, lineHeight, bottomMargin) = level switch
            {
                "2" => ("20px", "1.3", "12px"),
                "3" => ("16px", "1.4", "8px"),
                _ => ("24px", "1.25", "16px")
            };

            string tag = level switch
            {
                "2" => "h2",
                "3" => "h3",
                _ => "h1"
            };

            return $"""<{tag} style="margin:0 0 {bottomMargin} 0;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif;font-size:{fontSize};font-weight:700;line-height:{lineHeight};color:{color};text-align:{align};">{innerText}</{tag}>""";
        });

        // 3. Expand <Divider />
        html = DividerRegex.Replace(html, match =>
        {
            var attrs = ParseAttributes(match.Groups[1].Value);
            string color = attrs.GetValueOrDefault("color", "#e5e7eb");
            string margin = attrs.GetValueOrDefault("margin", "24px 0");
            string width = attrs.GetValueOrDefault("width", "100%");

            return $"""<hr style="border:none;border-top:1px solid {color};margin:{margin};width:{width};" />""";
        });

        // 4. Expand <Spacer />
        html = SpacerRegex.Replace(html, match =>
        {
            var attrs = ParseAttributes(match.Groups[1].Value);
            string heightStr = attrs.GetValueOrDefault("height", "20");
            if (heightStr.EndsWith("px", StringComparison.OrdinalIgnoreCase))
            {
                heightStr = heightStr[..^2];
            }

            if (!int.TryParse(heightStr, out int height))
            {
                height = 20;
            }

            return $"""<div style="height:{height}px;line-height:{height}px;font-size:1px;mso-line-height-rule:exactly;">&nbsp;</div>""";
        });

        // 5. Expand <Image />
        html = ImageRegex.Replace(html, match =>
        {
            var attrs = ParseAttributes(match.Groups[1].Value);
            string src = attrs.GetValueOrDefault("src", "");
            string alt = attrs.GetValueOrDefault("alt", "");
            string width = attrs.ContainsKey("width") ? $" width=\"{attrs["width"]}\"" : "";
            string height = attrs.ContainsKey("height") ? $" height=\"{attrs["height"]}\"" : "";

            return $"""<img src="{src}" alt="{alt}"{width}{height} style="display:block;outline:none;border:none;text-decoration:none;max-width:100%;" />""";
        });

        // 6. Expand <T.Try Key="key" /> and <T.Try("key") />
        MatchEvaluator resolveKey = match =>
        {
            string key = match.Groups[1].Value;
            if (localeResolver != null)
            {
                string? resolved = localeResolver.Resolve(key, culture);
                if (resolved != null)
                {
                    return resolved;
                }

                if (strictMode)
                {
                    throw new MissingLocaleKeyException(key, culture ?? "default");
                }
            }

            return key;
        };

        html = TTryTagRegex.Replace(html, resolveKey);
        html = TTryMethodRegex.Replace(html, resolveKey);

        return html;
    }

    private static Dictionary<string, string> ParseAttributes(string attrString)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(attrString))
        {
            return result;
        }

        var regex = new Regex(@"([a-zA-Z0-9_\-:]+)(?:\s*=\s*(?:""([^""]*)""|'([^']*)'|([^\s>]+)))?", RegexOptions.Compiled);
        var matches = regex.Matches(attrString);

        foreach (Match match in matches)
        {
            string key = match.Groups[1].Value;
            string val = match.Groups[2].Success ? match.Groups[2].Value :
                         match.Groups[3].Success ? match.Groups[3].Value :
                         match.Groups[4].Success ? match.Groups[4].Value : "";

            result[key] = val;
        }

        return result;
    }
}
