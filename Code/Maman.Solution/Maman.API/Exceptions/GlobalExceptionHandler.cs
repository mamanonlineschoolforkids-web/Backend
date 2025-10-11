using Microsoft.AspNetCore.Diagnostics;
using Maman.API.Exceptions;
using System.Text.Json;

namespace Maman.API.Exceptions;

public class GlobalExceptionHandler : IExceptionHandler
{
	private readonly ILogger<GlobalExceptionHandler> _logger;
	private readonly IHostEnvironment _env;

	public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment env)
	{
		_logger = logger;
		_env = env;
	}

	public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
	{
		_logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);

		var statusCode = exception switch
		{
			UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
			KeyNotFoundException => StatusCodes.Status404NotFound,
			// Add your custom BaseException if you have one
			_ => StatusCodes.Status500InternalServerError
		};

		var details = _env.IsDevelopment() ? exception.StackTrace : null;
		var response = new BaseErrorResponse(statusCode, details: details);

		context.Response.StatusCode = statusCode;
		context.Response.ContentType = "application/json";

		var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
		await context.Response.WriteAsync(JsonSerializer.Serialize(response, options), cancellationToken);

		return true;
	}
}