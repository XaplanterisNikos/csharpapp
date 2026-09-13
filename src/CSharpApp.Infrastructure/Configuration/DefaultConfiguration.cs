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
		// Bind and validate RestApiSettings at startup to ensure required values are available.
		services.AddOptions<RestApiSettings>()
		.Bind(configuration.GetSection(nameof(RestApiSettings)))
		.Validate(s => !string.IsNullOrWhiteSpace(s.BaseUrl), "RestApiSettings.BaseUrl is required.")
		.Validate(s => !string.IsNullOrWhiteSpace(s.Products), "RestApiSettings.Products is required.")
		.Validate(s => !string.IsNullOrWhiteSpace(s.Categories), "RestApiSettings.Categories is required.")
		.Validate(s => !string.IsNullOrWhiteSpace(s.Auth), "RestApiSettings.Auth is required.")
		.Validate(s => !string.IsNullOrWhiteSpace(s.Username), "RestApiSettings.Username is required.")
		.Validate(s => !string.IsNullOrWhiteSpace(s.Password), "RestApiSettings.Password is required.")
		.ValidateOnStart();

		services.Configure<HttpClientSettings>(configuration.GetSection(nameof(HttpClientSettings)));

        return services;
    }
}