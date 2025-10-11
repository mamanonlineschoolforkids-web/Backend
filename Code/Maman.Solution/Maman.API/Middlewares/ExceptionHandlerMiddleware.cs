using Maman.API.Errors;
using System.Net;
using System.Text.Json;

namespace Maman.API.Middlewares;

public class ExceptionHandlerMiddleware
{
	private readonly RequestDelegate next;
	private readonly IHostEnvironment env;
	private readonly ILogger<ExceptionHandlerMiddleware> logger;

	public ExceptionHandlerMiddleware(RequestDelegate next, IHostEnvironment env, ILogger<ExceptionHandlerMiddleware> logger)
	{
		this.next=next;
		this.env=env;
		this.logger=logger;
	}

	public async Task InvokeAsync(HttpContext HttpContext)
	{
		try
		{
			await next.Invoke(HttpContext);
		}
		catch (Exception ex)
		{
			/// Log The Exception
			logger.LogError(ex, "Unhandled exception occurred: {Message}", ex.Message);

			/// HEADER
			HttpContext.Response.ContentType = "application/json";
			HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

			/// BODY
			var response = env.IsDevelopment() ? new ExceptionResponse(500, ex.Message, ex.StackTrace)
												: new ExceptionResponse(500);

			var options = new JsonSerializerOptions()
			{
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase
			};

			var bodyAsString = JsonSerializer.Serialize(response);

			await HttpContext.Response.WriteAsync(bodyAsString);

		}
	}
}