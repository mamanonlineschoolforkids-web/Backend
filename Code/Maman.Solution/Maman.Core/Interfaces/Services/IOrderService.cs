using Maman.Core.Entities;

namespace Maman.Core.Interfaces.Services;

public interface IOrderService
{
	Task UpdateProductNameAsync(string productId, string newName);  // no transaction needed
	Task<Order> CreateOrderAsync(string productId, int quantity); // transaction needed
}