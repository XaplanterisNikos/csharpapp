using System.Diagnostics;

namespace CSharpApp.Api.Middleware;

/// <summary>
/// Measures how long each request takes and logs itw method , path , status code and elapsed milliseconds.
/// </summary>
public class RequestPerformanceMiddleware
{
	#region Fields

	private readonly RequestDelegate _next;
	private readonly ILogger<RequestPerformanceMiddleware> _logger;

	#endregion

	#region Constructor

	/// <summary>
	/// Initializes a new instance of the <see cref="RequestPerformanceMiddleware"/> class.
	/// </summary>
	/// <param name="next">The next component in the request pipeline.</param>
	/// <param name="logger">The logger used to record request performance.</param>
	public RequestPerformanceMiddleware(
		RequestDelegate next, 
		ILogger<RequestPerformanceMiddleware> logger)
	{
		_next = next;
		_logger = logger;
	}

	#endregion

	#region Public methods

	public async Task InvokeAsync(HttpContext context)
	{
		var stopwatch = Stopwatch.StartNew();
		try
		{
			// Hand the request to the rest of the pipeline.
			await _next(context);
		}
		finally
		{
			// Always log the timing, even if something downstream threw.
			stopwatch.Stop();

			_logger.LogInformation(
				"HTTP {Method} {Path} responded {StatusCode} in {ElapsedMs} ms.",
				context.Request.Method,
				context.Request.Path,
				context.Response.StatusCode, 
				stopwatch.ElapsedMilliseconds);
		}
	}
	#endregion
}
