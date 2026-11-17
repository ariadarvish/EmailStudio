namespace EmailStudio;

/// <summary>
/// Represents an email draft containing subject, rendered HTML body, and plain-text fallback.
/// </summary>
/// <param name="Subject">The email subject line.</param>
/// <param name="HtmlBody">The rendered HTML email body.</param>
/// <param name="TextBody">The plain text alternative body.</param>
public sealed record EmailDraft(string Subject, string HtmlBody, string TextBody);

