namespace Maman.API.Middlewares;

public static class MiddlewareExtensions
{
	public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
	{
		return app.UseMiddleware<GlobalExceptionMiddleware>();
	}

	public static IApplicationBuilder UsePerformanceLogging(this IApplicationBuilder app)
	{
		return app.UseMiddleware<PerformanceMiddleware>();
	}

	public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
	{
		return app.UseMiddleware<RequestLoggingMiddleware>();
	}

	public static IApplicationBuilder UseCustomLocalization(this IApplicationBuilder app)
	{
		return app.UseMiddleware<LocalizationMiddleware>();
	}
}