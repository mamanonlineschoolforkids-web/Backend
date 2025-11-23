using Microsoft.AspNetCore.Mvc.Filters;
using System.Diagnostics;

namespace Maman.API.Filters;

public class PerformanceFilter : IAsyncActionFilter
{
	private readonly ILogger<PerformanceFilter> _logger;

	public PerformanceFilter(ILogger<PerformanceFilter> logger)
	{
		_logger = logger;
	}

	public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
	{
		var stopwatch = Stopwatch.StartNew();
		var actionName = $"{context.Controller.GetType().Name}.{context.ActionDescriptor.DisplayName}";

		_logger.LogInformation("Action started: {ActionName}", actionName);

		var resultContext = await next();

		stopwatch.Stop();
		var elapsedMs = stopwatch.ElapsedMilliseconds;

		_logger.LogInformation(
			"Action completed: {ActionName} in {ElapsedMs}ms",
			actionName,
			elapsedMs
		);

		if (elapsedMs > 3000) // Warn if action takes more than 3 seconds
		{
			_logger.LogWarning(
				"Slow action detected: {ActionName} took {ElapsedMs}ms",
				actionName,
				elapsedMs
			);
		}
	}
}
