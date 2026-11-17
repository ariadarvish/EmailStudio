using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace EmailStudio.Preview;

/// <summary>
/// Monitors template and locale directories with debounced change notification.
/// </summary>
public sealed class TemplateWatcher : IDisposable
{
    private readonly PreviewServerOptions _options;
    private readonly WebSocketHub _hub;
    private readonly TemplateService _templateService;
    private readonly FileSystemWatcher? _templateWatcher;
    private readonly FileSystemWatcher? _localeWatcher;
    private readonly Timer _debounceTimer;
    private string? _pendingTemplateName;
    private bool _pendingGlobalReload;
    private readonly object _lock = new();

    public TemplateWatcher(PreviewServerOptions options, WebSocketHub hub, TemplateService templateService)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _hub = hub ?? throw new ArgumentNullException(nameof(hub));
        _templateService = templateService ?? throw new ArgumentNullException(nameof(templateService));

        _debounceTimer = new Timer(OnDebounceElapsed, null, Timeout.Infinite, Timeout.Infinite);

        if (Directory.Exists(_options.TemplateDirectory))
        {
            _templateWatcher = new FileSystemWatcher(_options.TemplateDirectory)
            {
                IncludeSubdirectories = true,
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.Size,
                Filter = "*.*",
                EnableRaisingEvents = true
            };

            _templateWatcher.Changed += OnFileChanged;
            _templateWatcher.Created += OnFileChanged;
            _templateWatcher.Deleted += OnFileChanged;
            _templateWatcher.Renamed += (s, e) => OnFileChanged(s, e);
        }

        if (Directory.Exists(_options.LocalesDirectory))
        {
            _localeWatcher = new FileSystemWatcher(_options.LocalesDirectory)
            {
                IncludeSubdirectories = true,
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName,
                Filter = "*.json",
                EnableRaisingEvents = true
            };

            _localeWatcher.Changed += OnLocaleChanged;
            _localeWatcher.Created += OnLocaleChanged;
            _localeWatcher.Deleted += OnLocaleChanged;
            _localeWatcher.Renamed += (s, e) => OnLocaleChanged(s, e);
        }
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        string ext = Path.GetExtension(e.FullPath);
        string name = Path.GetFileName(e.FullPath);

        // Ignore temporary editor files
        if (name.StartsWith("~") || name.StartsWith(".") || ext.Equals(".tmp", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (!ext.Equals(".cshtml", StringComparison.OrdinalIgnoreCase) &&
            !ext.Equals(".json", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        lock (_lock)
        {
            string templateName = Path.GetFileNameWithoutExtension(name);
            if (templateName.EndsWith(".sample", StringComparison.OrdinalIgnoreCase))
            {
                templateName = Path.GetFileNameWithoutExtension(templateName);
            }

            if (name.StartsWith("_") || name.Contains("Layout") || name.Contains("Partial"))
            {
                _pendingGlobalReload = true;
            }
            else
            {
                _pendingTemplateName = templateName;
            }

            // Reset debounce timer
            _debounceTimer.Change(_options.DebounceMs, Timeout.Infinite);
        }
    }

    private void OnLocaleChanged(object sender, FileSystemEventArgs e)
    {
        lock (_lock)
        {
            _templateService.ReloadLocales();
            _pendingGlobalReload = true;
            _debounceTimer.Change(_options.DebounceMs, Timeout.Infinite);
        }
    }

    private void OnDebounceElapsed(object? state)
    {
        string? targetTemplate;
        bool globalReload;

        lock (_lock)
        {
            targetTemplate = _pendingGlobalReload ? null : _pendingTemplateName;
            globalReload = _pendingGlobalReload;
            _pendingTemplateName = null;
            _pendingGlobalReload = false;
        }

        _ = _hub.BroadcastReloadAsync(targetTemplate);
    }

    public void Dispose()
    {
        _debounceTimer.Dispose();
        _templateWatcher?.Dispose();
        _localeWatcher?.Dispose();
    }
}

