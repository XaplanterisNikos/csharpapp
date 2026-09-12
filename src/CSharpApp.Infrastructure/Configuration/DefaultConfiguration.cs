namespace CSharpApp.Infrastructure.Configuration;

/// <summary>
/// Bind application settings to DI using the options pattern.
/// </summary>
public static class DefaultConfiguration
{
	/// <summary>
	/// Binds <see cref="RestApiSettings"/> and <see cref="HttpClientSettings"/> from configuration.
	/// </summary>
	/// <param name="services">The service collection to configure.</param>
	/// <param name="configuration">The application configuration to bind from.</param>
	/// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
	/// <remarks>
	/// IConfiguration is passed explicitly to avoid building a second service container (ASP0000).
	/// </remarks>
	public static IServiceCollection AddDefaultConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<RestApiSettings>(configuration!.GetSection(nameof(RestApiSettings)));
        services.Configure<HttpClientSettings>(configuration.GetSection(nameof(HttpClientSettings)));

        return services;
    }
}