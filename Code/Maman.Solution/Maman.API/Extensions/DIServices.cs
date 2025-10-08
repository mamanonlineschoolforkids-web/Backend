using Maman.Application.Services;
using Maman.Core;
using Maman.Core.Interfaces.Repositories;
using Maman.Core.Interfaces.Services;
using Maman.Infrastructure;
using Maman.Infrastructure.Repositories;

namespace Maman.API.Extensions;

public static class DIServices
{
	public static IServiceCollection AddApplicationServices(this IServiceCollection Services)
	{


		Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

		Services.AddScoped(typeof(IUnitOfWork), typeof(UnitOfWork));

		Services.AddScoped(typeof(IOrderService), typeof(OrderService));

		return Services;
	}

}
