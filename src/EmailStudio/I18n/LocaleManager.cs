using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace EmailStudio.I18n;

/// <summary>
/// Resolves localized string keys based on culture fallback chains.
/// </summary>
public interface ILocaleResolver
{
    /// <summary>
    /// Resolves a localization key for the specified culture.
    /// </summary>
    string? Resolve(string key, string? culture);
}

/// <summary>
/// In-memory locale manager supporting culture fallback chains (e.g., "de-DE" -> "de" -> default).
/// </summary>
public sealed class LocaleManager : ILocaleResolver
{
    private readonly ConcurrentDictionary<string, Dictionary<string, string>> _locales =
        new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Adds or updates translation entries for a culture.
    /// </summary>
    public void AddTranslations(string culture, IReadOnlyDictionary<string, string> translations)
    {
        ArgumentNullException.ThrowIfNull(culture);
        ArgumentNullException.ThrowIfNull(translations);

        var dict = _locales.GetOrAdd(culture, _ => new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));
        lock (dict)
        {
            foreach (var kvp in translations)
            {
                dict[kvp.Key] = kvp.Value;
            }
        }
    }

    /// <summary>
    /// Loads a JSON locale dictionary from a file.
    /// </summary>
    public void LoadJsonFile(string filePath, string? culture = null)
    {
        ArgumentNullException.ThrowIfNull(filePath);
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Locale file '{filePath}' not found.", filePath);
        }

        culture ??= Path.GetFileNameWithoutExtension(filePath);
        string json = File.ReadAllText(filePath);
        var entries = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
        if (entries != null)
        {
            AddTranslations(culture, entries);
        }
    }

    /// <summary>
    /// Resolves a localization key with fallback from "language-REGION" to "language".
    /// </summary>
    public string? Resolve(string key, string? culture)
    {
        ArgumentNullException.ThrowIfNull(key);

        if (string.IsNullOrWhiteSpace(culture))
        {
            // Try resolving from "default" or "en"
            if (_locales.TryGetValue("default", out var defaultDict) && defaultDict.TryGetValue(key, out var val))
            {
                return val;
            }
            if (_locales.TryGetValue("en", out var enDict) && enDict.TryGetValue(key, out var enVal))
            {
                return enVal;
            }
            return null;
        }

        // 1. Exact culture match (e.g., "de-DE")
        if (_locales.TryGetValue(culture, out var exactDict) && exactDict.TryGetValue(key, out var exactVal))
        {
            return exactVal;
        }

        // 2. Base language fallback (e.g., "de" from "de-DE")
        int dashIndex = culture.IndexOf('-');
        if (dashIndex > 0)
        {
            string baseCulture = culture[..dashIndex];
            if (_locales.TryGetValue(baseCulture, out var baseDict) && baseDict.TryGetValue(key, out var baseVal))
            {
                return baseVal;
            }
        }

        // 3. Global fallback to "default" or "en"
        if (_locales.TryGetValue("default", out var fallbackDict) && fallbackDict.TryGetValue(key, out var fallbackVal))
        {
            return fallbackVal;
        }

        return null;
    }
}
