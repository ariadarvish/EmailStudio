using EmailStudio;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for registering EmailStudio services in the application dependency injection container.
/// </summary>
public static class EmailStudioServiceCollectionExtensions
{
    /// <summary>
    /// Registers EmailStudio renderer and options services.
    /// </summary>
    /// <param name="services">Target service collection.</param>
    /// <param name="configure">Optional configuration action for rendering options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddEmailStudio(
        this IServiceCollection services,
        Action<EmailRenderOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var options = new EmailRenderOptions();
        configure?.Invoke(options);

        services.TryAddSingleton(options);
        services.TryAddSingleton<IEmailRenderer>(sp => new EmailRenderer(sp.GetService<EmailRenderOptions>() ?? options));

        return services;
    }
}
