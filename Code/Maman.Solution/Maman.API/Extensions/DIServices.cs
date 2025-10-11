using Maman.API.Exceptions;
using Maman.Application.Services;
using Maman.Core;
using Maman.Core.Interfaces.Services;
using Maman.Infrastructure;
using Maman.Infrastructure.Data;
using StackExchange.Redis;

namespace Maman.API.Extensions;

public static class DIServices
{
	public static IServiceCollection AddApplicationServices(this IServiceCollection Services , IConfiguration Configuration)
	{

		Services.Configure<MongoDbSettings>(Configuration.GetSection("MongoDbSettings"));
		Services.AddScoped<MongoDbContext>();


		Services.AddSingleton(typeof(ICacheService), typeof(CacheService));

		Services.AddScoped(typeof(IUnitOfWork), typeof(UnitOfWork));

		Services.AddScoped(typeof(IOrderService), typeof(OrderService));

		Services.AddExceptionHandler<GlobalExceptionHandler>();

		#region Validation Error Handling

		Services.Configure<ApiBehaviorOptions>(options =>
		{
			options.InvalidModelStateResponseFactory = (actionContext) =>
			{
				var errors = actionContext.ModelState.Where(p => p.Value.Errors.Count() > 0)
													.SelectMany(p => p.Value.Errors)
													.Select(e => e.ErrorMessage)
													.ToArray();

				var validationErrorResponse = new BaseErrorResponse(400, "A Validation error response occurred", null, errors);
			

				return new BadRequestObjectResult(validationErrorResponse);
			};
		});
		#endregion

		#region In-Memory Cache
		Services.AddOutputCache(); // Enables the middleware
		Services.AddMemoryCache();
		#endregion

		#region Redis Cache
		Services.AddSingleton<IConnectionMultiplexer>(provider =>
		{
			var connectionString = Configuration.GetConnectionString("Redis");
			return ConnectionMultiplexer.Connect(connectionString);
		});
		#endregion

		return Services;
	}

}
