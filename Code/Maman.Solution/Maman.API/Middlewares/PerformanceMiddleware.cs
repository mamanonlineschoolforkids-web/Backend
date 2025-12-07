using System.Diagnostics;

namespace Maman.API.Middlewares;

public class PerformanceMiddleware
{
	private readonly RequestDelegate _next;
	private readonly ILogger<PerformanceMiddleware> _logger;
	private readonly int _slowRequestThresholdMs;

	public PerformanceMiddleware(
		RequestDelegate next,
		ILogger<PerformanceMiddleware> logger,
		IConfiguration configuration)
	{
		_next = next;
		_logger = logger;
		_slowRequestThresholdMs = configuration.GetValue<int>("Performance:SlowRequestThresholdMs", 5000);
	}

	public async Task InvokeAsync(HttpContext context)
	{
		var stopwatch = Stopwatch.StartNew();

		try
		{
			await _next(context);
		}
		finally
		{
			stopwatch.Stop();
			var elapsedMs = stopwatch.ElapsedMilliseconds;

			if (elapsedMs > _slowRequestThresholdMs)
			{
				_logger.LogWarning(
					"Slow request detected: {Method} {Path} took {ElapsedMs}ms. Status: {StatusCode}",
					context.Request.Method,
					context.Request.Path,
					elapsedMs,
					context.Response.StatusCode
				);
			}

			// Add performance header
			context.Response.Headers.Add("X-Response-Time-Ms", elapsedMs.ToString());
		}
	}
}