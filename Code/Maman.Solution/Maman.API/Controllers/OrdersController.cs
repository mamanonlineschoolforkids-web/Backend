using Maman.Application.DTOs;
using Maman.Core.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Maman.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrdersController : ControllerBase
{
	private readonly IOrderService _orderService;

	public OrdersController(IOrderService orderService)
	{
		_orderService = orderService;
	}

	[HttpPut("products/{productId}/name")]
	public async Task<IActionResult> UpdateProductName(string productId, [FromBody] UpdateNameRequest request)
	{
		try
		{
			await _orderService.UpdateProductNameAsync(productId, request.NewName);
			return Ok(new { message = "Product name updated successfully." });
		}
		catch (Exception ex)
		{
			return BadRequest(ex.Message);
		}
	}

	[HttpPost]
	public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
	{
		try
		{
			var newOrder = await _orderService.CreateOrderAsync(request.ProductId, request.Quantity);
			// On success, return the created order with its new ID.
			return CreatedAtAction(nameof(CreateOrder), new { id = newOrder.Id }, newOrder);
		}
		catch (Exception ex)
		{
			// This will catch both "Product not found" and "Insufficient stock" errors.
			return BadRequest(new { error = ex.Message });
		}
	}
}
