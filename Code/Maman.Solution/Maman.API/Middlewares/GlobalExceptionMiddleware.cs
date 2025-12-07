using Maman.Application.DTOs.Common;
using System.Net;
using System.Text.Json;

namespace Maman.API.Middlewares;

public class GlobalExceptionMiddleware
{
	private readonly RequestDelegate _next;
	private readonly ILogger<GlobalExceptionMiddleware> _logger;
	private readonly IHostEnvironment _environment;

	public GlobalExceptionMiddleware(
		RequestDelegate next,
		ILogger<GlobalExceptionMiddleware> logger,
		IHostEnvironment environment)
	{
		_next = next;
		_logger = logger;
		_environment = environment;
	}

	public async Task InvokeAsync(HttpContext context)
	{
		try
		{
			await _next(context);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "An unhandled exception occurred. Request: {Method} {Path}",
				context.Request.Method, context.Request.Path);

			await HandleExceptionAsync(context, ex);
		}
	}

	private async Task HandleExceptionAsync(HttpContext context, Exception exception)
	{
		context.Response.ContentType = "application/json";
		context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

		var response = new ApiResponseDto<object>
		{
			Success = false,
			Message = "An error occurred while processing your request.",
			Errors = new List<string>()
		};

		// Add detailed error message in development
		if (_environment.IsDevelopment())
		{
			response.Errors.Add(exception.Message);
			response.Errors.Add(exception.StackTrace ?? string.Empty);
		}

		var options = new JsonSerializerOptions
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		};

		var json = JsonSerializer.Serialize(response, options);
		await context.Response.WriteAsync(json);
	}
}
