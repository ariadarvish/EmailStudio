namespace EmailStudio;

/// <summary>
/// Contract for dispatching typed model instances to generated email renderers.
/// </summary>
public interface IEmailRenderer
{
    /// <summary>
    /// Renders an email draft for the specified model and optional culture.
    /// </summary>
    /// <typeparam name="TModel">The template model type.</typeparam>
    /// <param name="model">The model instance with template data.</param>
    /// <param name="culture">Optional target culture (e.g. "de-DE").</param>
    /// <returns>An <see cref="EmailDraft"/> containing subject, HTML, and plain text.</returns>
    EmailDraft Render<TModel>(TModel model, string? culture = null);
}

