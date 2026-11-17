using System;
using System.Collections.Generic;
using EmailStudio.I18n;
using EmailStudio.SizeGuard;

namespace EmailStudio;

/// <summary>
/// Options configuring the EmailStudio rendering pipeline.
/// </summary>
public sealed class EmailRenderOptions
{
    /// <summary>
    /// Target culture for string localization (e.g. "de-DE", "fr-FR").
    /// </summary>
    public string? Culture { get; set; }

    /// <summary>
    /// Strict mode enforces that missing localization keys raise TPL009 errors.
    /// </summary>
    public bool StrictMode { get; set; }

    /// <summary>
    /// Warning threshold in bytes for Gmail size guard (default 90 KB).
    /// </summary>
    public long WarningThresholdBytes { get; set; } = EmailSizeGuard.DefaultWarningThresholdBytes;

    /// <summary>
    /// Maximum allowed HTML size in bytes before failing (default 102 KB Gmail clipping threshold).
    /// </summary>
    public long MaxThresholdBytes { get; set; } = EmailSizeGuard.DefaultMaxThresholdBytes;

    /// <summary>
    /// Locale manager for resolving localized strings.
    /// </summary>
    public ILocaleResolver? LocaleResolver { get; set; }

    /// <summary>
    /// Delegate to locate and load partial template content by path or name.
    /// </summary>
    public Func<string, string?>? PartialResolver { get; set; }

    /// <summary>
    /// Delegate to locate and load layout template content by path or name.
    /// </summary>
    public Func<string, string?>? LayoutResolver { get; set; }

    /// <summary>
    /// Callback invoked when a diagnostic warning is emitted.
    /// </summary>
    public Action<TemplateDiagnostic>? OnDiagnostic { get; set; }
}
