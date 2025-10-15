namespace Maman.API.Helpers;

public class CachingAttribute : Attribute, IAsyncActionFilter
{
	private readonly int timeOutInSeconds;

	public CachingAttribute(int timeOutInSeconds)
	{
		this.timeOutInSeconds=timeOutInSeconds;
	}
	public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
	{
		var _cacheService = context.HttpContext.RequestServices.GetRequiredService<ICacheService>();
		// Ask CLR for injecting Explicitly

		var key = GenerateCacheKeyFromRequest(context.HttpContext.Request);

		var response = await _cacheService.GetCachedResponseAsync(key);

		if (!string.IsNullOrEmpty(response))
		{
			var result = new ContentResult()
			{
				Content = response,
				ContentType = "application/json",
				StatusCode = 200
			};

			context.Result = result;
			return;
		}

		var executedActionContext = await next.Invoke();

		if (executedActionContext.Result is OkObjectResult okObjectResult && okObjectResult.Value is not null)
		{
			await _cacheService.CacheResponseAsync(key, okObjectResult.Value, TimeSpan.FromSeconds(timeOutInSeconds));
		}
	}

	private string GenerateCacheKeyFromRequest(HttpRequest request)
	{
		var keyBuilder = new StringBuilder();

		keyBuilder.Append(request.Path);

		foreach (var (key, value) in request.Query.OrderBy(x => x.Key))
		{
			keyBuilder.Append($"|{key}-{value}");
		}

		return keyBuilder.ToString();
	}
}
