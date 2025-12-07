using System.Globalization;

namespace Maman.API.Middlewares;

public class LocalizationMiddleware
{
	private readonly RequestDelegate _next;
	private readonly ILogger<LocalizationMiddleware> _logger;

	public LocalizationMiddleware(RequestDelegate next, ILogger<LocalizationMiddleware> logger)
	{
		_next = next;
		_logger = logger;
	}

	public async Task InvokeAsync(HttpContext context)
	{
		// Try to get language from header
		var acceptLanguage = context.Request.Headers["Accept-Language"].FirstOrDefault();

		// Try to get from JWT token claim
		var preferredLanguage = context.User?.FindFirst("PreferredLanguage")?.Value;

		var language = preferredLanguage ?? acceptLanguage ?? "en";

		// Parse language code (e.g., "en-US" -> "en")
		if (language.Contains('-'))
		{
			language = language.Split('-')[0];
		}

		try
		{
			var culture = new CultureInfo(language);
			CultureInfo.CurrentCulture = culture;
			CultureInfo.CurrentUICulture = culture;

			_logger.LogDebug("Culture set to: {Culture}", culture.Name);
		}
		catch (CultureNotFoundException)
		{
			_logger.LogWarning("Invalid culture: {Language}. Defaulting to 'en'", language);
			var defaultCulture = new CultureInfo("en");
			CultureInfo.CurrentCulture = defaultCulture;
			CultureInfo.CurrentUICulture = defaultCulture;
		}

		await _next(context);
	}
}