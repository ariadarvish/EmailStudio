using System;
using System.Collections.Concurrent;
using System.IO;

namespace EmailStudio;

/// <summary>
/// Default implementation of <see cref="IEmailRenderer"/> that renders templates using <see cref="EmailEngine"/>.
/// </summary>
public sealed class EmailRenderer : IEmailRenderer
{
    private readonly EmailRenderOptions _options;
    private readonly ConcurrentDictionary<Type, Func<object, string?, EmailDraft>> _modelRenderers = new();
    private readonly ConcurrentDictionary<string, string> _templateCache = new(StringComparer.OrdinalIgnoreCase);

    public EmailRenderer(EmailRenderOptions? options = null)
    {
        _options = options ?? new EmailRenderOptions();
    }

    /// <summary>
    /// Registers a template string for a specific model type.
    /// </summary>
    public void RegisterTemplate<TModel>(string templateContent, string templateName = "Template")
    {
        _modelRenderers[typeof(TModel)] = (model, culture) =>
        {
            var opt = new EmailRenderOptions
            {
                Culture = culture ?? _options.Culture,
                StrictMode = _options.StrictMode,
                WarningThresholdBytes = _options.WarningThresholdBytes,
                MaxThresholdBytes = _options.MaxThresholdBytes,
                LocaleResolver = _options.LocaleResolver,
                LayoutResolver = _options.LayoutResolver,
                PartialResolver = _options.PartialResolver,
                OnDiagnostic = _options.OnDiagnostic
            };
            return EmailEngine.Render(templateContent, model, opt, templateName);
        };
    }

    /// <inheritdoc/>
    public EmailDraft Render<TModel>(TModel model, string? culture = null)
    {
        ArgumentNullException.ThrowIfNull(model);

        if (_modelRenderers.TryGetValue(typeof(TModel), out var renderer))
        {
            return renderer(model, culture);
        }

        throw new InvalidOperationException(
            $"No template registered for model type '{typeof(TModel).FullName}'. Ensure a template is registered or use generated EmailTemplates.");
    }
}

