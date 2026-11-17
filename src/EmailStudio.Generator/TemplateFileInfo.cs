using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace EmailStudio.Generator;

/// <summary>
/// Immutable representation of an analyzed template file for incremental caching.
/// </summary>
public sealed class TemplateFileInfo : IEquatable<TemplateFileInfo>
{
    public string FilePath { get; }
    public string FileName { get; }
    public string TemplateName { get; }
    public bool IsLayout { get; }
    public bool IsPartial { get; }
    public string Content { get; }
    public string? Subject { get; }
    public string? Layout { get; }
    public string? ModelType { get; }
    public string? PlainText { get; }
    public ImmutableArray<string> ReferencedProperties { get; }
    public ImmutableArray<string> ReferencedPartials { get; }
    public ImmutableArray<string> ReferencedLocaleKeys { get; }
    public string? ParseError { get; }
    public int? ParseErrorLine { get; }
    public string? UnsupportedSyntax { get; }
    public int? UnsupportedSyntaxLine { get; }

    public TemplateFileInfo(
        string filePath,
        string fileName,
        string templateName,
        bool isLayout,
        bool isPartial,
        string content,
        string? subject,
        string? layout,
        string? modelType,
        string? plainText,
        ImmutableArray<string> referencedProperties,
        ImmutableArray<string> referencedPartials,
        ImmutableArray<string> referencedLocaleKeys,
        string? parseError = null,
        int? parseErrorLine = null,
        string? unsupportedSyntax = null,
        int? unsupportedSyntaxLine = null)
    {
        FilePath = filePath;
        FileName = fileName;
        TemplateName = templateName;
        IsLayout = isLayout;
        IsPartial = isPartial;
        Content = content;
        Subject = subject;
        Layout = layout;
        ModelType = modelType;
        PlainText = plainText;
        ReferencedProperties = referencedProperties;
        ReferencedPartials = referencedPartials;
        ReferencedLocaleKeys = referencedLocaleKeys;
        ParseError = parseError;
        ParseErrorLine = parseErrorLine;
        UnsupportedSyntax = unsupportedSyntax;
        UnsupportedSyntaxLine = unsupportedSyntaxLine;
    }

    public bool Equals(TemplateFileInfo? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return FilePath == other.FilePath &&
               Content == other.Content &&
               Subject == other.Subject &&
               Layout == other.Layout &&
               ModelType == other.ModelType &&
               ParseError == other.ParseError &&
               UnsupportedSyntax == other.UnsupportedSyntax;
    }

    public override bool Equals(object? obj) => Equals(obj as TemplateFileInfo);

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = (hash * 23) + (FilePath != null ? FilePath.GetHashCode() : 0);
            hash = (hash * 23) + (Content != null ? Content.GetHashCode() : 0);
            return hash;
        }
    }
}
