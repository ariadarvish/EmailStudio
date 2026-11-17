using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using EmailStudio.I18n;
using EmailStudio.Parsing;

namespace EmailStudio.Preview;

public sealed record TemplateSummary(
    string Name,
    string RelativePath,
    string? Subject,
    string? Layout,
    string? ModelType,
    DateTime LastModified,
    bool HasSampleData);

public sealed record TemplateRenderResult(
    bool Success,
    string TemplateName,
    EmailDraft? Draft,
    TemplateDiagnostic? Diagnostic,
    DateTime LastModified,
    string? Subject = null,
    string? Preheader = null,
    long ByteSize = 0,
    string FormattedSize = "0 KB",
    bool IsSizeWarning = false);

/// <summary>
/// Service that scans, parses, and renders templates and sample data from the workspace.
/// </summary>
public sealed class TemplateService
{
    private readonly PreviewServerOptions _options;
    private readonly LocaleManager _localeManager = new();

    public TemplateService(PreviewServerOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        ReloadLocales();
    }

    public void ReloadLocales()
    {
        if (Directory.Exists(_options.LocalesDirectory))
        {
            foreach (var file in Directory.GetFiles(_options.LocalesDirectory, "*.json"))
            {
                try
                {
                    _localeManager.LoadJsonFile(file);
                }
                catch
                {
                    // Ignore malformed locale during reload
                }
            }
        }
    }

    public IReadOnlyList<TemplateSummary> GetTemplates()
    {
        var list = new List<TemplateSummary>();
        if (!Directory.Exists(_options.TemplateDirectory))
        {
            return list;
        }

        var files = Directory.GetFiles(_options.TemplateDirectory, "*.cshtml", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            string fileName = Path.GetFileName(file);
            string relPath = Path.GetRelativePath(_options.TemplateDirectory, file);

            // Skip layouts and partials from root template list
            if (fileName.StartsWith("_") ||
                relPath.Contains("_Layouts", StringComparison.OrdinalIgnoreCase) ||
                relPath.Contains("_Partials", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            string name = Path.GetFileNameWithoutExtension(file);
            string content = File.ReadAllText(file);
            TemplateDocument doc;
            try
            {
                doc = FrontMatterParser.Parse(content, name);
            }
            catch
            {
                doc = new TemplateDocument(FrontMatter.Empty, content);
            }

            string samplePath = Path.ChangeExtension(file, ".sample.json");
            bool hasSample = File.Exists(samplePath);
            var lastMod = File.GetLastWriteTime(file);

            list.Add(new TemplateSummary(
                Name: name,
                RelativePath: relPath,
                Subject: doc.FrontMatter.Subject,
                Layout: doc.FrontMatter.Layout,
                ModelType: doc.ModelType,
                LastModified: lastMod,
                HasSampleData: hasSample));
        }

        return list.OrderBy(t => t.Name, StringComparer.OrdinalIgnoreCase).ToList();
    }

    public IReadOnlyList<string> GetAvailableCultures()
    {
        var cultures = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "en-US", "default" };
        if (Directory.Exists(_options.LocalesDirectory))
        {
            foreach (var file in Directory.GetFiles(_options.LocalesDirectory, "*.json"))
            {
                cultures.Add(Path.GetFileNameWithoutExtension(file));
            }
        }
        return cultures.OrderBy(c => c).ToList();
    }

    public TemplateRenderResult Render(string templateName, string? culture = null)
    {
        string? filePath = FindTemplateFile(templateName);
        if (filePath == null || !File.Exists(filePath))
        {
            return new TemplateRenderResult(
                Success: false,
                TemplateName: templateName,
                Draft: null,
                Diagnostic: new TemplateDiagnostic(
                    Code: "TPL003",
                    Message: $"Template '{templateName}' could not be found in '{_options.TemplateDirectory}'.",
                    Severity: DiagnosticSeverityLevel.Error),
                LastModified: DateTime.UtcNow);
        }

        string content = File.ReadAllText(filePath);
        var lastModified = File.GetLastWriteTime(filePath);

        // Load sample model
        object? model = null;
        string samplePath = Path.ChangeExtension(filePath, ".sample.json");
        if (File.Exists(samplePath))
        {
            try
            {
                string json = File.ReadAllText(samplePath);
                using var jsonDoc = JsonDocument.Parse(json);
                model = jsonDoc.RootElement.Clone();
            }
            catch (Exception ex)
            {
                return new TemplateRenderResult(
                    Success: false,
                    TemplateName: templateName,
                    Draft: null,
                    Diagnostic: new TemplateDiagnostic(
                        Code: "TPL004",
                        Message: $"Invalid sample JSON in '{Path.GetFileName(samplePath)}': {ex.Message}",
                        Severity: DiagnosticSeverityLevel.Warning,
                        FilePath: samplePath),
                    LastModified: lastModified);
            }
        }

        var renderOptions = new EmailRenderOptions
        {
            Culture = culture,
            LocaleResolver = _localeManager,
            LayoutResolver = ResolveLayout,
            PartialResolver = ResolvePartial
        };

        try
        {
            var draft = EmailEngine.Render(content, model, renderOptions, templateName);
            var parsedDoc = FrontMatterParser.Parse(content, templateName);
            string? preheader = null;
            if (parsedDoc.FrontMatter.CustomProperties.TryGetValue("preheader", out string? rawPreheader))
            {
                preheader = EmailStudio.TemplateEngine.TemplateEvaluator.InterpolateSubject(rawPreheader, model);
            }
            long byteSize = System.Text.Encoding.UTF8.GetByteCount(draft.HtmlBody);
            string formattedSize = $"{(double)byteSize / 1024:F1} KB";
            bool isWarning = byteSize >= 90 * 1024;

            return new TemplateRenderResult(
                Success: true,
                TemplateName: templateName,
                Draft: draft,
                Diagnostic: null,
                LastModified: lastModified,
                Subject: draft.Subject,
                Preheader: preheader,
                ByteSize: byteSize,
                FormattedSize: formattedSize,
                IsSizeWarning: isWarning);
        }
        catch (EmailStudioException ex)
        {
            return new TemplateRenderResult(
                Success: false,
                TemplateName: templateName,
                Draft: null,
                Diagnostic: new TemplateDiagnostic(
                    Code: ex.DiagnosticCode,
                    Message: ex.Message,
                    Severity: DiagnosticSeverityLevel.Error,
                    FilePath: filePath,
                    LineNumber: ex.LineNumber),
                LastModified: lastModified);
        }
        catch (Exception ex)
        {
            return new TemplateRenderResult(
                Success: false,
                TemplateName: templateName,
                Draft: null,
                Diagnostic: new TemplateDiagnostic(
                    Code: "TPL001",
                    Message: ex.Message,
                    Severity: DiagnosticSeverityLevel.Error,
                    FilePath: filePath),
                LastModified: lastModified);
        }
    }

    private string? FindTemplateFile(string templateName)
    {
        if (!Directory.Exists(_options.TemplateDirectory))
        {
            return null;
        }

        string direct = Path.Combine(_options.TemplateDirectory, templateName + ".cshtml");
        if (File.Exists(direct))
        {
            return direct;
        }

        return Directory.GetFiles(_options.TemplateDirectory, templateName + ".cshtml", SearchOption.AllDirectories)
            .FirstOrDefault();
    }

    private string? ResolveLayout(string layoutName)
    {
        string clean = layoutName.EndsWith(".cshtml", StringComparison.OrdinalIgnoreCase)
            ? layoutName
            : layoutName + ".cshtml";

        string direct = Path.Combine(_options.TemplateDirectory, clean);
        if (File.Exists(direct)) return File.ReadAllText(direct);

        string inLayouts = Path.Combine(_options.TemplateDirectory, "_Layouts", Path.GetFileName(clean));
        if (File.Exists(inLayouts)) return File.ReadAllText(inLayouts);

        var match = Directory.GetFiles(_options.TemplateDirectory, Path.GetFileName(clean), SearchOption.AllDirectories)
            .FirstOrDefault();

        return match != null ? File.ReadAllText(match) : null;
    }

    private string? ResolvePartial(string partialName)
    {
        string clean = partialName.EndsWith(".cshtml", StringComparison.OrdinalIgnoreCase)
            ? partialName
            : partialName + ".cshtml";

        string direct = Path.Combine(_options.TemplateDirectory, clean);
        if (File.Exists(direct)) return File.ReadAllText(direct);

        string inPartials = Path.Combine(_options.TemplateDirectory, "_Partials", Path.GetFileName(clean));
        if (File.Exists(inPartials)) return File.ReadAllText(inPartials);

        var match = Directory.GetFiles(_options.TemplateDirectory, Path.GetFileName(clean), SearchOption.AllDirectories)
            .FirstOrDefault();

        return match != null ? File.ReadAllText(match) : null;
    }
}

