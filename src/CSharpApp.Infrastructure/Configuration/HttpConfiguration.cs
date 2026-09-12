namespace CSharpApp.Infrastructure.Configuration;

/// <summary>
/// Registers the types <see cref="HttpClient"/> instances used to reach the external REST API
/// with their handler lifetime and resilience policies
/// </summary>
public static class HttpConfiguration
{
	#region Public API

	/// <summary>
	/// Registers all typed API clients (<see cref="IProductsService"/>, <see cref="ICategoriesService"/>)
	/// backed by <see cref="IHttpClientFactory"/>, sharing the same base address, handler lifetime
	/// and transient-fault retry policy.
	/// </summary>
	/// <param name="services">The service collection to add the client to.</param>
	/// <param name="configuration">The application configuration used to read HTTP settings.</param>
	/// <returns>The same <see cref="IServiceCollection"/> for chaining</returns>
	/// <remarks>
	/// Typed clients are transient, allowing handler rotation to pick up DNS changes.
	/// </remarks>
	public static IServiceCollection AddHttpConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
		var httpClientSettings = configuration.GetSection(nameof(HttpClientSettings))
			.Get<HttpClientSettings>() ?? new HttpClientSettings();

		// Centralize shared API client registration in a helper
		AddApiClient<IProductsService, ProductsService>(services, httpClientSettings);
		AddApiClient<ICategoriesService, CategoriesService>(services, httpClientSettings);

		return services;
    }
	#endregion

	#region Private Helpers
	/// <summary>
	/// Registers a single typed client with the shared base address, handler lifetime and retry policy.
	/// </summary>
	/// <typeparam name="TInterface">The service interface to register.</typeparam>
	/// <typeparam name="TImplementation">The concrete typed-client implementation.</typeparam>
	/// <param name="services">The service collection.</param>
	/// <param name="settings">The HTTP client settings driving lifetime and retries.</param>
	/// <returns>The <see cref="IHttpClientBuilder"/> for the registered client.</returns>
	private static IHttpClientBuilder AddApiClient<TInterface, TImplementation>(
		IServiceCollection services,
		HttpClientSettings settings)
		where TInterface : class
		where TImplementation : class, TInterface
	{
		return services.AddHttpClient<TInterface, TImplementation>((serviceProvider, client) =>
		{
			var restApiSettings =
				serviceProvider.GetRequiredService<IOptions<RestApiSettings>>().Value;

			client.BaseAddress = new Uri(restApiSettings.BaseUrl!);
		})
			.SetHandlerLifetime(TimeSpan.FromMinutes(settings.LifeTime))
			.AddPolicyHandler((serviceProvider, _) =>
			{
				var s = serviceProvider.GetRequiredService<IOptions<HttpClientSettings>>().Value;
				return GetRetryPolicy(s);
			});
	}
	#endregion

	#region Policies
	/// <summary>
	/// Builds a retry policy for transient HTTP errors using exponential backoff based on
	/// <see cref="HttpClientSettings.SleepDuration"/>.
	/// </summary>
	/// <param name="settings">The HTTP client settings that drive retry count and delay.</param>
	/// <returns>An async Polly policy for <see cref="HttpResponseMessage"/>.</returns>
	private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(HttpClientSettings settings)
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(
            retryCount: settings.RetryCount,
            sleepDurationProvider: attempt =>
            TimeSpan.FromMilliseconds(settings.SleepDuration * Math.Pow(2, attempt - 1)));
    }
    #endregion
}