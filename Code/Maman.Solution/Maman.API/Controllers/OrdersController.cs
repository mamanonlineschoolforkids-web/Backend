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
	[ProducesResponseType( StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(BaseErrorResponse), StatusCodes.Status400BadRequest)]
	//[CachingAttribute(600)]
	//[OutputCache(Duration = 600, NoStore = false)]
	public async Task<IActionResult> UpdateProductName(string productId, [FromBody] UpdateNameDTO request)
	{
		try
		{
			await _orderService.UpdateProductNameAsync(productId, request.NewName);
			return Ok(new { message = "Product name updated successfully." });
		}
		catch (Exception ex)
		{
			return BadRequest(new BaseErrorResponse (400 , ex.Message));
		}
	}

	[HttpPost]
	[ProducesResponseType(StatusCodes.Status201Created)]
	[ProducesResponseType(typeof(BaseErrorResponse), StatusCodes.Status400BadRequest)]
	[ServiceFilter(typeof(ValidationFilter<CreateOrderDTO>))]
	public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDTO request)
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
			return BadRequest(new BaseErrorResponse(400, ex.Message));
		}
	}
}
