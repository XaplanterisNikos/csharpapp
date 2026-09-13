namespace CSharpApp.Api.Middleware;

/// <summary>
/// Registration helpers for <see cref="RequestPerformanceMiddleware"/>.
/// </summary>
public static class RequestPerformanceMiddlewareExtensions
{
	/// <summary>
	/// Adds request performance logging to the application pipeline.
	/// </summary>
	/// <param name="app">The application builder.</param>
	/// <returns>The same <see cref="IApplicationBuilder"/> for chaining.</returns>
	public static IApplicationBuilder UseRequestPerformanceLogging(this IApplicationBuilder app)
	{
		return app.UseMiddleware<RequestPerformanceMiddleware>();
	}
}
