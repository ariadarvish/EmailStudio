using EmailStudio;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for registering EmailStudio services.
/// </summary>
public static class EmailStudioServiceCollectionExtensions
{
    /// <summary>
    /// Adds EmailStudio renderer services to the service collection.
    /// </summary>
    /// <param name="services">The target service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddEmailStudio(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        return services;
    }
}

