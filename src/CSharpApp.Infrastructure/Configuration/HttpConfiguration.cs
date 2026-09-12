using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;

namespace CSharpApp.Infrastructure.Configuration;

/// <summary>
/// Registers the types <see cref="HttpClient"/> instances used to reach the external REST API
/// with their handler lifetime and resilience policies
/// </summary>
public static class HttpConfiguration
{
	#region Public API

	/// <summary>
	/// Registers <see cref="IProductsService"/> as a typed client backed by
	/// <see cref="IHttpClientFactory"/>, configuring its base address, handler lifetime
	/// and a transient-fault retry policy.
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
		// Read HttpClient settings once during registration for the handler lifetime.
		var httpClientSettings = configuration.GetSection(nameof(HttpClientSettings))
            .Get<HttpClientSettings>() ?? new HttpClientSettings();

		// Register ProductsService as a typed client with an HttpClient managed by IHttpClientFactory.
		services.AddHttpClient<IProductsService, ProductsService>((serviceProvider, client) =>
        {
            var restApiSettings =
                serviceProvider.GetRequiredService<IOptions<RestApiSettings>>().Value;
            client.BaseAddress = new Uri(restApiSettings.BaseUrl!);
        })
			// Rotate the underlying handler to pick up DNS changes.
			.SetHandlerLifetime(TimeSpan.FromMinutes(httpClientSettings.LifeTime))
			// Retry transient failures, including 5xx responses, timeouts, and network errors via Polly.
			.AddPolicyHandler((serviceProvider, _) =>
            {
                var settings =
                    serviceProvider.GetRequiredService<IOptions<HttpClientSettings>>().Value;
                return GetRetryPolicy(settings);
            });

        return services;
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