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
#region Auth

/// store refresh tokens in Redis.
/// Input Sanitization: Use FluentValidation for DTOs to prevent injection.
///Error Handling: Custom ProblemDetails responses (e.g., "Invalid credentials") to avoid leaks; 
/// log with Serilog.
/// 

//1. /auth/register

//-DTO: { Name, Email, Password, Country, Role, ...Profile }.
//-Hash Email, PhoneNumber, PasswordHashed (BCrypt.Net-Next).
//- Validate unique EmailHashed/PhoneNumberHashed via IMongoCollection.FindAsync + indexes.
//- For Students: Set optional Parent ref, DailyUsageLimitInMinutes.
//- For ServiceProviders: Set EarnerProfile.VerificationStatus = "pending".
//- Generate email verification token (JWT with expiry); send via MailKit/SendGrid.
//- Return JWT + user summary (exclude sensitive fields via [JsonIgnore]).
//-Rate limit: AspNetCoreRateLimit(5/min by IP/Email).


//2. /auth/login
//-DTO: { Email, Password }
//or { GoogleId, ... }.
//-Query by EmailHashed; verify PasswordHashed with BCrypt.Net.BCrypt.Verify.
//- For Google: Check GoogleId; auto-register if absent(Google.Apis.Auth).
//- Increment FailedLoginAttempts; if >3, lock 15min (check LastFailedLogin).
//- On success: Reset attempts, update LastLogin/FirstLogin = false, generate JWT (payload: Id, Role, Status via System.IdentityModel.Tokens.Jwt).
//- For Admins: Enforce 2FA if TwoFactorEnabled (OTP via Authy.NET).
//- Return { AccessToken, RefreshToken, User: { Name, Role, ... } }.
//-Block if Status == "suspended" or "deleted".
#endregion

//git reset --soft HEAD~1
//git push origin main --force

//Add indexes on Email, RefreshToken.Token, RefreshToken.UserId

//Consider using MongoDB's TTL index for automatic token cleanup
//Use UpdateOneAsync with $set instead of ReplaceOneAsync for better performance


// Rate Limiting
// Serilog logging ( events , slow requests )
// Localization
// Global exception handling 
// Input validation
// Performance evaluating
// Hybrid cashing L1 , L2 ( in memory , redis )
// Auditing
// Azure file storage
// Elastic search
// jwt token
// email service
// google login
// refresh token
// 2 Factor auth 
// soft delete
// Auth : register , login , logout , refresh token , revoke token , reset password , request reset password , verify email , delete account , edit profile , restore account , permentatnt delete account , share profile