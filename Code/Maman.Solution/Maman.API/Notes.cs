#region Caching

#region 1. In Memory Cache
// Store inside the application memory
// Very fast
// Data is lost if the app restarts
// Small Data Sizes
#endregion

#region 2. Distributed Cache
// An external service (like Redis) that is shared by multiple servers
// Scalable applications with multiple servers
#endregion

#region Content Delivery Network (CDN)
//A geographically distributed network of servers that cache static content like images
//Websites with global users, to serve content from a server physically closer to the user, reducing load times.
#endregion

#endregion

#region In-Memory Caching
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
#endregion

#region Validation

#region Data Annotations

#endregion
//using FluentValidation;
//using Ma_man.Core.Entities;

//namespace Ma_man.Application.Validators
//{
//	public class UserValidator : AbstractValidator<User>
//	{
//		public UserValidator()
//		{
//			var allowedRoles = new List<string>() { "student", "parent", "contentCreator", "specialist", "admin" };

//			RuleFor(u => u.Name)
//				.NotEmpty()
//				.MaximumLength(100);

//			RuleFor(u => u.Role)
//				.NotEmpty()
//				.Must((role => allowedRoles.Contains(role)))
//				.WithMessage($"Role must be one of: {string.Join(", ", allowedRoles)}");

//			RuleFor(u => u.Status)
//				.IsInEnum();



//			RuleFor(u => u).Must(u => u.Role == "student" ? (u as Student)?.StudentProfile.Dob != default : true).WithMessage("Student Date of Birth is required."); ;

//			// Add more validations based on schema

//		}
//	}
//}
#endregion