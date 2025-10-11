using Maman.API.Errors;
using Maman.Application.Services;
using Maman.Core;
using Maman.Core.Interfaces.Repositories;
using Maman.Core.Interfaces.Services;
using Maman.Infrastructure;
using Maman.Infrastructure.Data;
using Maman.Infrastructure.Repositories;

namespace Maman.API.Extensions;

public static class DIServices
{
	public static IServiceCollection AddApplicationServices(this IServiceCollection Services , IConfiguration Configuration)
	{

		Services.Configure<MongoDbSettings>(Configuration.GetSection("MongoDbSettings"));
		Services.AddSingleton<MongoDbContext>();

		//Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

		Services.AddScoped(typeof(IUnitOfWork), typeof(UnitOfWork));

		Services.AddScoped(typeof(IOrderService), typeof(OrderService));

		#region Validation Error Handling

		Services.Configure<ApiBehaviorOptions>(options =>
		{
			options.InvalidModelStateResponseFactory = (actionContext) =>
			{
				var errors = actionContext.ModelState.Where(p => p.Value.Errors.Count() > 0)
													.SelectMany(p => p.Value.Errors)
													.Select(e => e.ErrorMessage)
													.ToArray();

				var validationErrorResponse = new ValidationErrorResponse()
				{
					Errors = errors
				};

				return new BadRequestObjectResult(validationErrorResponse);
			};
		});
		#endregion

		return Services;
	}

}
