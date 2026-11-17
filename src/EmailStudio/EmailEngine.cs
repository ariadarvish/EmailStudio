using System;
using System.Text.RegularExpressions;
using EmailStudio.Components;
using EmailStudio.Inlining;
using EmailStudio.Parsing;
using EmailStudio.SizeGuard;
using EmailStudio.TemplateEngine;
using EmailStudio.Text;

namespace EmailStudio;

/// <summary>
/// Core rendering engine orchestrating the complete EmailStudio template pipeline.
/// </summary>
public static class EmailEngine
{
    private static readonly Regex RenderBodyRegex = new(
        @"@RenderBody\(\)",
        RegexOptions.Compiled);

    /// <summary>
    /// Compiles and renders an email template into an <see cref="EmailDraft"/>.
    /// </summary>
    /// <param name="templateContent">Raw .cshtml template content.</param>
    /// <param name="model">Model instance containing template data.</param>
    /// <param name="options">Optional rendering options.</param>
    /// <param name="templateName">Name of the template for diagnostics and errors.</param>
    /// <returns>A rendered <see cref="EmailDraft"/> with subject, HTML body, and plain text.</returns>
    public static EmailDraft Render(
        string templateContent,
        object? model,
        EmailRenderOptions? options = null,
        string templateName = "Template")
    {
        ArgumentNullException.ThrowIfNull(templateContent);

        options ??= new EmailRenderOptions();

        // 1. Parse front matter
        var document = FrontMatterParser.Parse(templateContent, templateName);

        // Subject is mandatory on email templates
        if (string.IsNullOrWhiteSpace(document.FrontMatter.Subject))
        {
            throw new MissingSubjectException(templateName);
        }

        string rawBody = document.Body;

        // 2. Wrap with layout if specified
        if (!string.IsNullOrWhiteSpace(document.FrontMatter.Layout))
        {
            string layoutName = document.FrontMatter.Layout;
            string? layoutContent = options.LayoutResolver?.Invoke(layoutName);

            if (layoutContent == null)
            {
                throw new TemplateNotFoundException("layout", layoutName);
            }

            // Strip any front matter from layout itself
            var layoutDoc = FrontMatterParser.Parse(layoutContent, layoutName);
            if (!RenderBodyRegex.IsMatch(layoutDoc.Body))
            {
                throw new TemplateParseException($"Layout '{layoutName}' is missing '@RenderBody()' placeholder.");
            }

            rawBody = RenderBodyRegex.Replace(layoutDoc.Body, rawBody);
        }

        // 3. Evaluate syntax subset (model expressions, @if, @foreach, partials)
        string evaluated = TemplateEvaluator.Evaluate(
            content: rawBody,
            model: model,
            partialResolver: options.PartialResolver);

        // 4. Expand built-in email components (<Button>, <Heading>, <Divider>, <Spacer>, <Image>, <T.Try>)
        string expanded = ComponentExpander.Expand(
            html: evaluated,
            localeResolver: options.LocaleResolver,
            culture: options.Culture,
            strictMode: options.StrictMode);

        // 5. CSS Inlining via PreMailer.Net
        string inlinedHtml = CssInliner.InlineCss(expanded);

        // 6. Plain text generation
        string textBody;
        if (!string.IsNullOrWhiteSpace(document.FrontMatter.PlainText))
        {
            textBody = TemplateEvaluator.Evaluate(document.FrontMatter.PlainText, model, options.PartialResolver);
        }
        else
        {
            textBody = PlainTextGenerator.FromHtml(inlinedHtml);
        }

        // 7. Subject placeholder interpolation: "Welcome, {Name}!"
        string subject = TemplateEvaluator.InterpolateSubject(document.FrontMatter.Subject, model);

        // 8. Gmail Size Guard verification
        EmailSizeGuard.Validate(
            html: inlinedHtml,
            templateName: templateName,
            warningThresholdBytes: options.WarningThresholdBytes,
            maxThresholdBytes: options.MaxThresholdBytes,
            onWarning: options.OnDiagnostic);

        return new EmailDraft(
            Subject: subject,
            HtmlBody: inlinedHtml,
            TextBody: textBody);
    }
}
