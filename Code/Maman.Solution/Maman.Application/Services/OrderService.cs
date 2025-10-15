using Order = Maman.Core.Entities.Order;

namespace Maman.Application.Services;

public class OrderService : IOrderService
{
	private readonly IUnitOfWork _unitOfWork;

	public OrderService(IUnitOfWork unitOfWork)
	{
		_unitOfWork = unitOfWork;
	}
	
	public async Task UpdateProductNameAsync(string productId, string newName)
	{
		var productRepo = _unitOfWork.Repository<Product>();

		var product = await productRepo.GetByIdAsync(productId);

		if (product == null)
		{
			throw new Exception($"Product with id {productId} not found.");
		}

		product.Name = newName;
		
		await productRepo.UpdateAsync(product);
	}

	public async Task<Order> CreateOrderAsync(string productId, int quantity)
	{
		var newOrder = new Order { ProductId = productId, Quantity = quantity };

		// We wrap the entire set of operations in ExecuteInTransactionAsync.
		await _unitOfWork.ExecuteInTransactionAsync(async session =>
		{
			var productRepo = _unitOfWork.Repository<Product>();
			var orderRepo = _unitOfWork.Repository<Order>();

			var product = await productRepo.GetByIdAsync(productId);

			if (product == null)
			{
				throw new Exception("Product not found.");
			}
			if (product.Stock < quantity)
			{
				throw new InvalidOperationException("Insufficient stock to place order.");
			}

			// 1. Decrease the product stock
			product.Stock -= quantity;
			await productRepo.UpdateAsync(product, session); // <-- Pass the session

			// 2. Create the order record
			await orderRepo.AddAsync(newOrder, session);      // <-- Pass the session
		});

		// The transaction has been committed, so we can safely return the new order.
		return newOrder;
	}
}