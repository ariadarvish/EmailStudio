namespace EmailStudio.Preview;

/// <summary>
/// Configuration options for the EmailStudio preview server.
/// </summary>
public sealed class PreviewServerOptions
{
    /// <summary>
    /// Port number the preview server listens on (default 5173).
    /// </summary>
    public int Port { get; set; } = 5173;

    /// <summary>
    /// Host address to bind to (default "localhost").
    /// </summary>
    public string Host { get; set; } = "localhost";

    /// <summary>
    /// Directory containing template files (default "Templates").
    /// </summary>
    public string TemplateDirectory { get; set; } = "Templates";

    /// <summary>
    /// Directory containing locale dictionary files (default "Locales").
    /// </summary>
    public string LocalesDirectory { get; set; } = "Locales";

    /// <summary>
    /// Local SMTP catcher host for "Send to Inbox" (default "localhost").
    /// </summary>
    public string SmtpHost { get; set; } = "localhost";

    /// <summary>
    /// Local SMTP catcher port (default 1025 for Mailpit).
    /// </summary>
    public int SmtpPort { get; set; } = 1025;

    /// <summary>
    /// File watcher debounce period in milliseconds (default 200 ms).
    /// </summary>
    public int DebounceMs { get; set; } = 200;
}

