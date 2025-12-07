namespace Maman.API.Middlewares;

public class RequestLoggingMiddleware
{
	private readonly RequestDelegate _next;
	private readonly ILogger<RequestLoggingMiddleware> _logger;

	public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
	{
		_next = next;
		_logger = logger;
	}

	public async Task InvokeAsync(HttpContext context)
	{
		var requestId = Guid.NewGuid().ToString();
		context.Items["RequestId"] = requestId;

		_logger.LogInformation(
			"Request started: {RequestId} {Method} {Path} from {IpAddress}",
			requestId,
			context.Request.Method,
			context.Request.Path,
			context.Connection.RemoteIpAddress
		);

		try
		{
			await _next(context);
		}
		finally
		{
			_logger.LogInformation(
				"Request completed: {RequestId} {Method} {Path} - Status: {StatusCode}",
				requestId,
				context.Request.Method,
				context.Request.Path,
				context.Response.StatusCode
			);
		}
	}
}

