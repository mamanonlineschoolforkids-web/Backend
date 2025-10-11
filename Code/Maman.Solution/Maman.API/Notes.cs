//// Check with auth
//// Cache for 10 minutes, no vary-by-query (static list)
//using Microsoft.AspNetCore.OutputCaching;

//[HttpGet]
//[OutputCache(Duration = 600, NoStore = false)] // 10 min = 600 sec
//public async Task<IActionResult> GetAll()
//{
//	var products = await _productService.GetAllAsync();
//	return Ok(products);
//}

//// Cache for 1 minute, vary by user ID (personalized)
//[HttpGet("{id}")]
//[OutputCache(Duration = 60, VaryByQueryKeys = ["id"])] // Vary by 'id' query param
//public async Task<IActionResult> GetById(int id)
//{
//	var product = await _productService.GetByIdAsync(id);
//	return product != null ? Ok(product) : NotFound();
//}

//// No cache for writes (POST/PUT/DELETE)
//[HttpPost]
//[OutputCache(NoStore = true)] // Or omit attribute to skip caching
//public async Task<IActionResult> Create(ProductDto dto)
//{
//	// ... create logic
//	return CreatedAtAction(nameof(GetById), new { id = newProduct.Id }, newProduct);
//}