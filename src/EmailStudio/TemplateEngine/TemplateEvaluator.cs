using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace EmailStudio.TemplateEngine;

/// <summary>
/// Evaluates the EmailStudio Razor-subset syntax on template bodies and layouts.
/// </summary>
public static partial class TemplateEvaluator
{
    private static readonly string[] ForbiddenDirectives =
    [
        "@functions",
        "@await",
        "@section",
        "@inject",
        "@inherits",
        "@implements"
    ];

    private static readonly Regex PartialTagRegex = new(
        @"<(?:Partial|Include)\s+(?:Name|Path)=""([^""]+)""\s*/?>",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Checks for forbidden Razor directives outside the supported subset.
    /// </summary>
    public static void ValidateSyntax(string content)
    {
        if (string.IsNullOrEmpty(content))
        {
            return;
        }

        foreach (string directive in ForbiddenDirectives)
        {
            int index = content.IndexOf(directive, StringComparison.OrdinalIgnoreCase);
            if (index >= 0)
            {
                int line = 1;
                for (int i = 0; i < index; i++)
                {
                    if (content[i] == '\n')
                    {
                        line++;
                    }
                }
                throw new UnsupportedSyntaxException(directive, line);
            }
        }
    }

    /// <summary>
    /// Evaluates model data, control flow (@if, @foreach), and expressions in template content.
    /// </summary>
    public static string Evaluate(
        string content,
        object? model,
        Func<string, string?>? partialResolver = null,
        Dictionary<string, object?>? localVariables = null)
    {
        if (string.IsNullOrEmpty(content))
        {
            return string.Empty;
        }

        // 1. Verify syntax subset
        ValidateSyntax(content);

        // 2. Expand partials: <Partial Name="_Partials/Header" />
        if (partialResolver != null)
        {
            content = PartialTagRegex.Replace(content, match =>
            {
                string partialPath = match.Groups[1].Value;
                string? partialContent = partialResolver(partialPath);
                if (partialContent == null)
                {
                    throw new TemplateNotFoundException("partial", partialPath);
                }
                return Evaluate(partialContent, model, partialResolver, localVariables);
            });
        }

        // 3. Process @if / else blocks
        content = ProcessIfBlocks(content, model, localVariables, partialResolver);

        // 4. Process @foreach loops
        content = ProcessForeachBlocks(content, model, localVariables, partialResolver);

        // 5. Replace model and variable expressions: @Model.Property or @varName.Property
        content = EvaluateExpressions(content, model, localVariables);

        return content;
    }

    /// <summary>
    /// Interpolates subject string with model placeholders (e.g. "Welcome, {Name}!").
    /// </summary>
    public static string InterpolateSubject(string subjectPattern, object? model)
    {
        if (string.IsNullOrEmpty(subjectPattern) || model == null)
        {
            return subjectPattern ?? string.Empty;
        }

        var regex = new Regex(@"\{([a-zA-Z0-9_.]+)\}", RegexOptions.Compiled);
        return regex.Replace(subjectPattern, match =>
        {
            string path = match.Groups[1].Value;
            if (path.StartsWith("Model.", StringComparison.OrdinalIgnoreCase))
            {
                path = path[6..];
            }

            object? val = ResolveValue(model, path);
            return val?.ToString() ?? string.Empty;
        });
    }

    private static string ProcessIfBlocks(
        string content,
        object? model,
        Dictionary<string, object?>? localVars,
        Func<string, string?>? partialResolver)
    {
        int index = 0;
        var sb = new StringBuilder();

        while (index < content.Length)
        {
            int ifIndex = content.IndexOf("@if", index, StringComparison.Ordinal);
            if (ifIndex < 0)
            {
                sb.Append(content.AsSpan(index));
                break;
            }

            // Append everything prior to @if
            sb.Append(content.AsSpan(index, ifIndex - index));

            // Find '(' of condition
            int condStart = content.IndexOf('(', ifIndex + 3);
            if (condStart < 0)
            {
                sb.Append(content.AsSpan(ifIndex));
                break;
            }

            int condEnd = FindMatchingDelimiter(content, condStart, '(', ')');
            if (condEnd < 0)
            {
                sb.Append(content.AsSpan(ifIndex));
                break;
            }

            string condition = content.Substring(condStart + 1, condEnd - condStart - 1);

            // Find '{' of if body
            int bodyStart = content.IndexOf('{', condEnd + 1);
            if (bodyStart < 0)
            {
                sb.Append(content.AsSpan(ifIndex));
                break;
            }

            int bodyEnd = FindMatchingDelimiter(content, bodyStart, '{', '}');
            if (bodyEnd < 0)
            {
                sb.Append(content.AsSpan(ifIndex));
                break;
            }

            string ifBody = content.Substring(bodyStart + 1, bodyEnd - bodyStart - 1);
            int nextIndex = bodyEnd + 1;

            // Check if followed by "else"
            string elseBody = string.Empty;
            int elseSearch = nextIndex;
            while (elseSearch < content.Length && char.IsWhiteSpace(content[elseSearch]))
            {
                elseSearch++;
            }

            if (elseSearch + 4 <= content.Length && content.Substring(elseSearch, 4) == "else")
            {
                int elseBodyStart = content.IndexOf('{', elseSearch + 4);
                if (elseBodyStart >= 0)
                {
                    int elseBodyEnd = FindMatchingDelimiter(content, elseBodyStart, '{', '}');
                    if (elseBodyEnd >= 0)
                    {
                        elseBody = content.Substring(elseBodyStart + 1, elseBodyEnd - elseBodyStart - 1);
                        nextIndex = elseBodyEnd + 1;
                    }
                }
            }

            bool conditionMet = EvaluateCondition(condition, model, localVars);
            string chosen = conditionMet ? ifBody : elseBody;

            sb.Append(Evaluate(chosen, model, partialResolver, localVars));
            index = nextIndex;
        }

        return sb.ToString();
    }

    private static string ProcessForeachBlocks(
        string content,
        object? model,
        Dictionary<string, object?>? localVars,
        Func<string, string?>? partialResolver)
    {
        int index = 0;
        var sb = new StringBuilder();

        while (index < content.Length)
        {
            int foreachIndex = content.IndexOf("@foreach", index, StringComparison.Ordinal);
            if (foreachIndex < 0)
            {
                sb.Append(content.AsSpan(index));
                break;
            }

            sb.Append(content.AsSpan(index, foreachIndex - index));

            int parenStart = content.IndexOf('(', foreachIndex + 8);
            if (parenStart < 0)
            {
                sb.Append(content.AsSpan(foreachIndex));
                break;
            }

            int parenEnd = FindMatchingDelimiter(content, parenStart, '(', ')');
            if (parenEnd < 0)
            {
                sb.Append(content.AsSpan(foreachIndex));
                break;
            }

            string iterationHeader = content.Substring(parenStart + 1, parenEnd - parenStart - 1).Trim();

            // Format: "var item in Collection" or "item in Collection"
            var parts = iterationHeader.Split([" in "], StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
            {
                sb.Append(content.AsSpan(foreachIndex, parenEnd - foreachIndex + 1));
                index = parenEnd + 1;
                continue;
            }

            string itemVar = parts[0].Trim();
            if (itemVar.StartsWith("var ", StringComparison.Ordinal))
            {
                itemVar = itemVar[4..].Trim();
            }

            string collectionExpr = parts[1].Trim();
            if (collectionExpr.StartsWith('@'))
            {
                collectionExpr = collectionExpr[1..];
            }
            if (collectionExpr.StartsWith("Model.", StringComparison.OrdinalIgnoreCase))
            {
                collectionExpr = collectionExpr[6..];
            }

            int bodyStart = content.IndexOf('{', parenEnd + 1);
            if (bodyStart < 0)
            {
                sb.Append(content.AsSpan(foreachIndex));
                break;
            }

            int bodyEnd = FindMatchingDelimiter(content, bodyStart, '{', '}');
            if (bodyEnd < 0)
            {
                sb.Append(content.AsSpan(foreachIndex));
                break;
            }

            string loopBody = content.Substring(bodyStart + 1, bodyEnd - bodyStart - 1);
            object? collectionObj = ResolveValue(model, collectionExpr, localVars);

            if (collectionObj is IEnumerable enumerable && collectionObj is not string)
            {
                foreach (object? item in enumerable)
                {
                    var scopedVars = localVars != null
                        ? new Dictionary<string, object?>(localVars, StringComparer.OrdinalIgnoreCase)
                        : new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

                    scopedVars[itemVar] = item;
                    sb.Append(Evaluate(loopBody, model, partialResolver, scopedVars));
                }
            }

            index = bodyEnd + 1;
        }

        return sb.ToString();
    }

    private static int FindMatchingDelimiter(string text, int openIndex, char openChar, char closeChar)
    {
        int depth = 0;
        for (int i = openIndex; i < text.Length; i++)
        {
            if (text[i] == openChar)
            {
                depth++;
            }
            else if (text[i] == closeChar)
            {
                depth--;
                if (depth == 0)
                {
                    return i;
                }
            }
        }
        return -1;
    }

    private static string EvaluateExpressions(
        string content,
        object? model,
        Dictionary<string, object?>? localVars)
    {
        var regex = new Regex(@"@([a-zA-Z0-9_]+(?:\.[a-zA-Z0-9_]+)*)", RegexOptions.Compiled);

        return regex.Replace(content, match =>
        {
            string expr = match.Groups[1].Value;

            if (expr is "if" or "else" or "foreach" or "model" or "RenderBody")
            {
                return match.Value;
            }

            string path = expr;
            if (path.StartsWith("Model.", StringComparison.OrdinalIgnoreCase))
            {
                path = path[6..];
            }

            object? val = ResolveValue(model, path, localVars);
            if (val == null)
            {
                return string.Empty;
            }

            return HtmlEncoder.Default.Encode(Convert.ToString(val, CultureInfo.InvariantCulture) ?? string.Empty);
        });
    }

    public static object? ResolveValue(
        object? root,
        string path,
        Dictionary<string, object?>? localVars = null)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return root;
        }

        string[] parts = path.Split('.');
        object? current = null;

        if (localVars != null && localVars.TryGetValue(parts[0], out var localVar))
        {
            current = localVar;
            if (parts.Length == 1)
            {
                return current;
            }
            parts = parts[1..];
        }
        else
        {
            current = root;
        }

        foreach (string part in parts)
        {
            if (current == null)
            {
                return null;
            }

            current = GetPropertyValue(current, part);
        }

        return current;
    }

    private static object? GetPropertyValue(object target, string propertyName)
    {
        if (target == null)
        {
            return null;
        }

        if (target is JsonElement jsonElement)
        {
            if (jsonElement.ValueKind == JsonValueKind.Object && jsonElement.TryGetProperty(propertyName, out var prop))
            {
                return prop.ValueKind switch
                {
                    JsonValueKind.String => prop.GetString(),
                    JsonValueKind.Number => prop.TryGetInt64(out long l) ? l : prop.GetDouble(),
                    JsonValueKind.True => true,
                    JsonValueKind.False => false,
                    JsonValueKind.Null => null,
                    _ => prop
                };
            }
            return null;
        }

        if (target is IDictionary<string, object> dict && dict.TryGetValue(propertyName, out var dictVal))
        {
            return dictVal;
        }

        if (target is IDictionary generalDict && generalDict.Contains(propertyName))
        {
            return generalDict[propertyName];
        }

        var propInfo = target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        if (propInfo != null)
        {
            return propInfo.GetValue(target);
        }

        var fieldInfo = target.GetType().GetField(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        if (fieldInfo != null)
        {
            return fieldInfo.GetValue(target);
        }

        return null;
    }

    private static bool EvaluateCondition(
        string condition,
        object? model,
        Dictionary<string, object?>? localVars)
    {
        condition = condition.Trim();

        bool negate = false;
        if (condition.StartsWith('!'))
        {
            negate = true;
            condition = condition[1..].Trim();
        }

        if (condition.Contains("=="))
        {
            string[] parts = condition.Split("==", 2);
            object? left = ResolveValue(model, NormalizeExpr(parts[0]), localVars);
            string right = parts[1].Trim().Trim('"', '\'');
            bool result = string.Equals(left?.ToString(), right, StringComparison.OrdinalIgnoreCase);
            return negate ? !result : result;
        }

        if (condition.Contains("!="))
        {
            string[] parts = condition.Split("!=", 2);
            object? left = ResolveValue(model, NormalizeExpr(parts[0]), localVars);
            string right = parts[1].Trim().Trim('"', '\'');
            bool result = !string.Equals(left?.ToString(), right, StringComparison.OrdinalIgnoreCase);
            return negate ? !result : result;
        }

        string normalized = NormalizeExpr(condition);
        object? val = ResolveValue(model, normalized, localVars);

        bool truthy = val switch
        {
            null => false,
            bool b => b,
            int i => i != 0,
            long l => l != 0,
            string s => !string.IsNullOrEmpty(s),
            ICollection c => c.Count > 0,
            _ => true
        };

        return negate ? !truthy : truthy;
    }

    private static string NormalizeExpr(string expr)
    {
        expr = expr.Trim();
        if (expr.StartsWith("Model.", StringComparison.OrdinalIgnoreCase))
        {
            expr = expr[6..];
        }
        return expr;
    }
}
