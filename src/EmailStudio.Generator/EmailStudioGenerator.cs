using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace EmailStudio.Generator;

/// <summary>
/// Incremental Roslyn source generator for EmailStudio templates.
/// Validates templates at compile time (TPL001-TPL009) and emits typed render methods.
/// </summary>
[Generator]
public sealed class EmailStudioGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // 1. Template files (.cshtml)
        var templateFiles = context.AdditionalTextsProvider
            .Where(static file => file.Path.EndsWith(".cshtml", StringComparison.OrdinalIgnoreCase))
            .Select(static (file, ct) =>
            {
                var text = file.GetText(ct);
                string content = text != null ? text.ToString() : string.Empty;
                return TemplateAnalyzer.Analyze(file.Path, content);
            });

        // 2. Sample data files (.sample.json)
        var sampleFiles = context.AdditionalTextsProvider
            .Where(static file => file.Path.EndsWith(".sample.json", StringComparison.OrdinalIgnoreCase))
            .Select(static (file, ct) =>
            {
                var text = file.GetText(ct);
                string content = text != null ? text.ToString() : string.Empty;
                return (Path: file.Path, Content: content);
            });

        // 3. Locale files (Locales/*.json)
        var localeFiles = context.AdditionalTextsProvider
            .Where(static file => file.Path.IndexOf("Locales", StringComparison.OrdinalIgnoreCase) >= 0 &&
                                  file.Path.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            .Select(static (file, ct) =>
            {
                var text = file.GetText(ct);
                string content = text != null ? text.ToString() : string.Empty;
                return (Path: file.Path, Content: content);
            });

        // Combine inputs with compilation
        var combined = templateFiles.Collect()
            .Combine(sampleFiles.Collect())
            .Combine(localeFiles.Collect())
            .Combine(context.CompilationProvider);

        context.RegisterSourceOutput(combined, static (spc, data) =>
        {
            var templates = data.Left.Left.Left;
            var samples = data.Left.Left.Right;
            var locales = data.Left.Right;
            var compilation = data.Right;

            if (templates.IsEmpty)
            {
                return;
            }

            var sampleDict = samples.ToDictionary(
                s => System.IO.Path.GetFileName(s.Path),
                s => s,
                StringComparer.OrdinalIgnoreCase);

            var localeKeySet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var loc in locales)
            {
                ExtractJsonKeys(loc.Content, localeKeySet);
            }

            // Map templates for layout and partial lookups
            var allTemplateNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var duplicateGroups = templates
                .Where(t => !t.IsLayout && !t.IsPartial)
                .GroupBy(t => t.TemplateName, StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1);

            foreach (var t in templates)
            {
                allTemplateNames.Add(t.TemplateName);
                allTemplateNames.Add(t.FileName);
                allTemplateNames.Add(System.IO.Path.GetFileNameWithoutExtension(t.FilePath));
            }

            // TPL006: Duplicate template names
            foreach (var dupGroup in duplicateGroups)
            {
                foreach (var dup in dupGroup.Skip(1))
                {
                    spc.ReportDiagnostic(Diagnostic.Create(
                        DiagnosticDescriptors.TPL006_DuplicateTemplateName,
                        Location.None,
                        dupGroup.Key,
                        dup.FilePath));
                }
            }

            // Validate each template
            foreach (var t in templates)
            {
                // TPL001: Parse error
                if (t.ParseError != null)
                {
                    spc.ReportDiagnostic(Diagnostic.Create(
                        DiagnosticDescriptors.TPL001_ParseError,
                        Location.None,
                        t.FileName,
                        t.ParseError));
                    continue;
                }

                // TPL008: Unsupported syntax
                if (t.UnsupportedSyntax != null)
                {
                    spc.ReportDiagnostic(Diagnostic.Create(
                        DiagnosticDescriptors.TPL008_UnsupportedSyntax,
                        Location.None,
                        t.UnsupportedSyntax));
                }

                // TPL005: Missing subject (only required for actual email templates, not layouts/partials)
                if (!t.IsLayout && !t.IsPartial && string.IsNullOrWhiteSpace(t.Subject))
                {
                    spc.ReportDiagnostic(Diagnostic.Create(
                        DiagnosticDescriptors.TPL005_MissingSubject,
                        Location.None,
                        t.TemplateName));
                }

                // TPL003: Layout not found
                if (!string.IsNullOrWhiteSpace(t.Layout))
                {
                    string layoutRef = t.Layout!;
                    string layoutClean = System.IO.Path.GetFileNameWithoutExtension(layoutRef);
                    if (!allTemplateNames.Contains(layoutRef) && !allTemplateNames.Contains(layoutClean))
                    {
                        spc.ReportDiagnostic(Diagnostic.Create(
                            DiagnosticDescriptors.TPL003_LayoutOrPartialNotFound,
                            Location.None,
                            "layout",
                            layoutRef));
                    }
                }

                // TPL003: Partials not found
                foreach (var partialRef in t.ReferencedPartials)
                {
                    string partialClean = System.IO.Path.GetFileNameWithoutExtension(partialRef);
                    if (!allTemplateNames.Contains(partialRef) && !allTemplateNames.Contains(partialClean))
                    {
                        spc.ReportDiagnostic(Diagnostic.Create(
                            DiagnosticDescriptors.TPL003_LayoutOrPartialNotFound,
                            Location.None,
                            "partial",
                            partialRef));
                    }
                }

                // TPL004: Sample JSON validation
                string expectedSampleName = t.TemplateName + ".sample.json";
                if (sampleDict.TryGetValue(expectedSampleName, out var sample))
                {
                    if (!IsValidJson(sample.Content, out string jsonErr))
                    {
                        spc.ReportDiagnostic(Diagnostic.Create(
                            DiagnosticDescriptors.TPL004_InvalidSampleJson,
                            Location.None,
                            sample.Path,
                            jsonErr));
                    }
                }

                // TPL002: Unknown property on model type
                if (!string.IsNullOrWhiteSpace(t.ModelType))
                {
                    var modelSymbol = compilation.GetTypeByMetadataName(t.ModelType!);
                    if (modelSymbol != null)
                    {
                        var availableProps = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                        CollectProperties(modelSymbol, availableProps);

                        foreach (var prop in t.ReferencedProperties)
                        {
                            if (!availableProps.Contains(prop))
                            {
                                spc.ReportDiagnostic(Diagnostic.Create(
                                    DiagnosticDescriptors.TPL002_UnknownModelProperty,
                                    Location.None,
                                    prop,
                                    t.ModelType));
                            }
                        }
                    }
                }

                // TPL007: Size threshold
                long byteCount = Encoding.UTF8.GetByteCount(t.Content);
                if (byteCount > 90 * 1024)
                {
                    spc.ReportDiagnostic(Diagnostic.Create(
                        DiagnosticDescriptors.TPL007_HtmlSizeExceeded,
                        Location.None,
                        t.TemplateName,
                        byteCount,
                        90 * 1024));
                }

                // TPL009: Missing locale key
                if (locales.Length > 0 && t.ReferencedLocaleKeys.Length > 0)
                {
                    foreach (var key in t.ReferencedLocaleKeys)
                    {
                        if (!localeKeySet.Contains(key))
                        {
                            spc.ReportDiagnostic(Diagnostic.Create(
                                DiagnosticDescriptors.TPL009_MissingLocaleKey,
                                Location.None,
                                key,
                                "locales"));
                        }
                    }
                }
            }

            // Emit generated C# code
            string code = TemplateCodeEmitter.Emit(templates);
            spc.AddSource("EmailStudio.EmailTemplates.g.cs", SourceText.From(code, Encoding.UTF8));
        });
    }

    private static void CollectProperties(ITypeSymbol symbol, HashSet<string> properties)
    {
        var current = symbol;
        while (current != null && current.SpecialType != SpecialType.System_Object)
        {
            foreach (var member in current.GetMembers())
            {
                if (member is IPropertySymbol || member is IFieldSymbol)
                {
                    properties.Add(member.Name);
                }
            }
            current = current.BaseType;
        }
    }

    private static bool IsValidJson(string json, out string error)
    {
        error = string.Empty;
        if (string.IsNullOrWhiteSpace(json))
        {
            error = "JSON content is empty.";
            return false;
        }

        string trimmed = json.Trim();
        if ((trimmed.StartsWith("{") && trimmed.EndsWith("}")) ||
            (trimmed.StartsWith("[") && trimmed.EndsWith("]")))
        {
            return true;
        }

        error = "JSON does not start with '{' or '['.";
        return false;
    }

    private static void ExtractJsonKeys(string json, HashSet<string> keys)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return;
        }

        // Fast regex key extractor for flat locale JSON dictionaries
        var regex = new Regex(@"""([^""]+)""\s*:", RegexOptions.Compiled);
        foreach (Match m in regex.Matches(json))
        {
            keys.Add(m.Groups[1].Value);
        }
    }
}
