using Maman.Application.DTOs.User;
using Maman.Application.Interfaces;
using Maman.Application.Services.Utility;
using Maman.Core.Enums;
using Maman.Core.Specifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;

namespace Maman.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[EnableRateLimiting("fixed")]
public class UserController : ControllerBase
{
	private readonly IUserService _userService;
	private readonly ICurrentUserService _currentUserService;
	private readonly ILogger<UserController> _logger;

	public UserController(
		IUserService userService,
		ICurrentUserService currentUserService,
		ILogger<UserController> logger)
	{
		_userService = userService;
		_currentUserService = currentUserService;
		_logger = logger;
	}

	/// Get current user profile
	[HttpGet("profile")]
	public async Task<IActionResult> GetProfile(CancellationToken cancellationToken)
	{
		var userId = _currentUserService.UserId!;
		var result = await _userService.GetProfileAsync(userId, cancellationToken);

		if (!result.Success)
			return NotFound(result);

		return Ok(result);
	}

	/// <summary>
	/// Update current user profile
	/// </summary>
	[HttpPut("profile")]
	public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto request, CancellationToken cancellationToken)
	{
		var userId = _currentUserService.UserId!;
		var result = await _userService.UpdateProfileAsync(userId, request, cancellationToken);

		if (!result.Success)
			return BadRequest(result);

		return Ok(result);
	}

	/// <summary>
	/// Change password
	/// </summary>
	[HttpPost("change-password")]
	public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto request, CancellationToken cancellationToken)
	{
		var userId = _currentUserService.UserId!;
		var ipAddress = _currentUserService.IpAddress ?? "Unknown";

		var result = await _userService.ChangePasswordAsync(userId, request, ipAddress, cancellationToken);

		if (!result.Success)
			return BadRequest(result);

		return Ok(result);
	}

	/// <summary>
	/// Upload profile picture
	/// </summary>
	[HttpPost("profile-picture")]
	[RequestSizeLimit(5 * 1024 * 1024)] // 5MB
	public async Task<IActionResult> UploadProfilePicture([FromForm] IFormFile file, CancellationToken cancellationToken)
	{
		var userId = _currentUserService.UserId!;
		var result = await _userService.UploadProfilePictureAsync(userId, file, cancellationToken);

		if (!result.Success)
			return BadRequest(result);

		return Ok(result);
	}

	/// <summary>
	/// Delete account (soft delete)
	/// </summary>
	[HttpDelete("account")]
	public async Task<IActionResult> DeleteAccount([FromBody] DeleteAccountDto request, CancellationToken cancellationToken)
	{
		var userId = _currentUserService.UserId!;
		var ipAddress = _currentUserService.IpAddress ?? "Unknown";

		var result = await _userService.DeleteAccountAsync(userId, ipAddress,request, cancellationToken);

		if (!result.Success)
			return BadRequest(result);

		return Ok(result);
	}

	/// <summary>
	/// Restore deleted account
	/// </summary>
	[HttpPost("restore-account")]
	[AllowAnonymous]
	public async Task<IActionResult> RestoreAccount([FromQuery] string userId, CancellationToken cancellationToken)
	{
		var result = await _userService.RestoreAccountAsync(userId, cancellationToken);

		if (!result.Success)
			return BadRequest(result);

		return Ok(result);
	}

	/// <summary>
	/// Permanently delete account (Admin only)
	/// </summary>
	[HttpDelete("permanent-delete/{userId}")]
	[Authorize(Roles = "admin")]
	public async Task<IActionResult> PermanentDeleteAccount(
		string userId,
		[FromBody] string reason,
		CancellationToken cancellationToken)
	{
		var adminUserId = _currentUserService.UserId!;
		var ipAddress = _currentUserService.IpAddress ?? "Unknown";

		var result = await _userService.PermanentDeleteAccountAsync(userId, adminUserId, ipAddress, reason, cancellationToken);

		if (!result.Success)
			return BadRequest(result);

		return Ok(result);
	}

	/// <summary>
	/// Share profile with selected fields
	/// </summary>
	[HttpPost("share-profile")]
	public async Task<IActionResult> ShareProfile([FromBody] ShareProfileDto request, CancellationToken cancellationToken)
	{
		var userId = _currentUserService.UserId!;
		var result = await _userService.ShareProfileAsync(userId, request, cancellationToken);

		if (!result.Success)
			return BadRequest(result);

		return Ok(result);
	}

	/// <summary>
	/// Get paginated users list (Admin only)
	/// </summary>
	[HttpGet]
	[Authorize(Roles = "admin")]
	public async Task<IActionResult> GetUsers(
		UserParams userParams,
		CancellationToken cancellationToken = default)
	{
		var result = await _userService.GetUsersAsync(userParams);

		if (!result.Success)
			return BadRequest(result);

		return Ok(result);
	}
}


