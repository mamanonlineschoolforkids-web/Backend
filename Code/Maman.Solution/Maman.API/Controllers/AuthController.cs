using Maman.API.Filters;
using Maman.Application.DTOs.Auth;
using Maman.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Maman.API.Controllers;
[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("fixed")]
public class AuthController : ControllerBase
{
	private readonly IAuthService _authService;
	private readonly ICurrentUserService _currentUserService;
	private readonly ILogger<AuthController> _logger;

	public AuthController(
		IAuthService authService,
		ICurrentUserService currentUserService,
		ILogger<AuthController> logger)
	{
		_authService = authService;
		_currentUserService = currentUserService;
		_logger = logger;
	}


	[HttpPost("register")]
	//[EnableRateLimiting("register")]
	[ServiceFilter(typeof(ValidationFilter<RegisterRequestDto>))]
	public async Task<IActionResult> Register([FromBody] RegisterRequestDto request, CancellationToken cancellationToken)
	{
		_logger.LogInformation("Registration attempt for email: {Email}", request.Email);
		var result = await _authService.RegisterAsync(request, cancellationToken);

		if (!result.Success)
			return BadRequest(result);

		return Ok(result);
	}

	[HttpPost("verify-email")]
	[ServiceFilter(typeof(ValidationFilter<VerifyEmailDto>))]
	public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailDto request, CancellationToken cancellationToken)
	{
		var result = await _authService.VerifyEmailAsync(request, cancellationToken);

		if (!result.Success)
			return BadRequest(result);

		return Ok(result);
	}

	[HttpPost("resend-verification")]
	[EnableRateLimiting("email-verification")]
	public async Task<IActionResult> ResendVerificationEmail([FromQuery] string email, CancellationToken cancellationToken)
	{
		var result = await _authService.ResendVerificationEmailAsync(email, cancellationToken);

		if (!result.Success)
			return BadRequest(result);

		return Ok(result);
	}

	[HttpPost("login")]
	[EnableRateLimiting("login")]
	[ServiceFilter(typeof(ValidationFilter<LoginRequestDto>))]
	public async Task<IActionResult> Login([FromBody] LoginRequestDto request, CancellationToken cancellationToken)
	{
		var ipAddress = _currentUserService.IpAddress ?? "Unknown";
		var userAgent = Request.Headers["User-Agent"].ToString();

		_logger.LogInformation("Login attempt for email: {Email} from IP: {IpAddress}", request.Email, ipAddress);

		var result = await _authService.LoginAsync(request, ipAddress, userAgent, cancellationToken);

		if (!result.Success)
			return Unauthorized(result);

		return Ok(result);
	}

	[HttpPost("request-password-reset")]
	[EnableRateLimiting("password-reset")]
	[ServiceFilter(typeof(ValidationFilter<RequestPasswordResetDto>))]
	public async Task<IActionResult> RequestPasswordReset([FromBody] RequestPasswordResetDto request, CancellationToken cancellationToken)
	{
		var result = await _authService.RequestPasswordResetAsync(request, cancellationToken);
		return Ok(result); // Always return 200 to prevent email enumeration
	}

	/// Reset password using token
	[HttpPost("reset-password")]
	[ServiceFilter(typeof(ValidationFilter<ResetPasswordDto>))]
	public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto request, CancellationToken cancellationToken)
	{
		var ipAddress = _currentUserService.IpAddress ?? "Unknown";

		var result = await _authService.ResetPasswordAsync(request, ipAddress, cancellationToken);

		if (!result.Success)
			return BadRequest(result);

		return Ok(result);
	}

	/// Refresh access token using refresh token
	[HttpPost("refresh-token")]
	[ServiceFilter(typeof(ValidationFilter<RefreshTokenRequestDto>))]
	public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request, CancellationToken cancellationToken)
	{
		var ipAddress = _currentUserService.IpAddress ?? "Unknown";
		var userAgent = Request.Headers["User-Agent"].ToString();

		var result = await _authService.RefreshTokenAsync(request.RefreshToken, ipAddress, userAgent, cancellationToken);

		if (!result.Success)
			return Unauthorized(result);

		return Ok(result);
	}

	/// Revoke a specific refresh token
	[HttpPost("revoke-token")]
	[ServiceFilter(typeof(ValidationFilter<RevokeTokenRequestDto>))]
	[Authorize]
	public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenRequestDto request, CancellationToken cancellationToken)
	{
		var result = await _authService.RevokeTokenAsync(request.RefreshToken, cancellationToken);

		if (!result.Success)
			return BadRequest(result);

		return Ok(result);
	}

	/// Logout current user (revoke all refresh tokens)
	[HttpPost("logout")]
	[Authorize]
	public async Task<IActionResult> Logout(CancellationToken cancellationToken)
	{
		var userId = _currentUserService.UserId!;
		var ipAddress = _currentUserService.IpAddress ?? "Unknown";
		var result = await _authService.LogoutAsync(userId, ipAddress, cancellationToken);

		if (!result.Success)
			return BadRequest(result);

		return Ok(result);
	}


	[HttpPost("google-login")]
	[EnableRateLimiting("login")]
	[ServiceFilter(typeof(ValidationFilter<GoogleLoginRequestDto>))]
	public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequestDto request, CancellationToken cancellationToken)
	{
		var ipAddress = _currentUserService.IpAddress ?? "Unknown";
		var userAgent = Request.Headers["User-Agent"].ToString();

		_logger.LogInformation("Google login attempt from IP: {IpAddress}", ipAddress);

		var result = await _authService.GoogleLoginAsync(request, ipAddress, userAgent, cancellationToken);

		if (!result.Success)
			return Unauthorized(result);

		return Ok(result);
	}

	

	/// Enable Two-Factor Authentication
	[HttpPost("enable-2fa")]
	[Authorize]
	[ServiceFilter(typeof(ValidationFilter<Enable2FADto>))]
	public async Task<IActionResult> Enable2FA([FromBody] Enable2FADto request, CancellationToken cancellationToken)
	{
		var userId = _currentUserService.UserId!;
		var result = await _authService.Enable2FAAsync(userId, request, cancellationToken);

		if (!result.Success)
			return BadRequest(result);

		return Ok(result);
	}

	/// Verify and activate Two-Factor Authentication
	[HttpPost("verify-2fa")]
	[ServiceFilter(typeof(ValidationFilter<Verify2FADto>))]
	[Authorize]
	public async Task<IActionResult> Verify2FA([FromBody] Verify2FADto request, CancellationToken cancellationToken)
	{
		var userId = _currentUserService.UserId!;
		var result = await _authService.Verify2FAAsync(userId, request, cancellationToken);

		if (!result.Success)
			return BadRequest(result);

		return Ok(result);
	}

	/// Disable Two-Factor Authentication
	[HttpPost("disable-2fa")]
	[ServiceFilter(typeof(ValidationFilter<Disable2FADto>))]
	[Authorize]
	public async Task<IActionResult> Disable2FA([FromBody] Disable2FADto request, CancellationToken cancellationToken)
	{
		var userId = _currentUserService.UserId!;
		var result = await _authService.Disable2FAAsync(userId, request, cancellationToken);

		if (!result.Success)
			return BadRequest(result);

		return Ok(result);
	}
}
