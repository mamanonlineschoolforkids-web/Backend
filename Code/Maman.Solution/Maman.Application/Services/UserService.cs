using Maman.Application.DTOs.Auth;
using Maman.Application.DTOs.Common;
using Maman.Application.DTOs.User;
using Maman.Application.Interfaces;
using Maman.Core.Entities.Auth;
using Maman.Core.Enums;
using Maman.Core.Interfaces;
using Maman.Core.Specifications;
using Maman.Localization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Maman.Application.Services;

public class UserService : IUserService
{
	private readonly IUnitOfWork _unitOfWork;
	private readonly IFileStorageService _fileStorageService;
	private readonly IEmailService _emailService;
	private readonly ICacheService _cacheService;
	private readonly IAuditService _auditService;
	private readonly IStringLocalizer<SharedResource> _localizer;
	private readonly ILogger<UserService> _logger;

	public UserService(
		IUnitOfWork unitOfWork,
		IFileStorageService fileStorageService,
		IEmailService emailService,
		ICacheService cacheService,
		IAuditService auditService,
		IStringLocalizer<SharedResource> localizer,
		ILogger<UserService> logger)
	{
		_unitOfWork = unitOfWork;
		_fileStorageService = fileStorageService;
		_emailService = emailService;
		_cacheService = cacheService;
		_auditService = auditService;
		_localizer = localizer;
		_logger = logger;
	}

	public async Task<ApiResponseDto<UserDto>> GetProfileAsync(string userId, CancellationToken cancellationToken = default)
	{
		try
		{
			// Try cache first
			var cachedUser = await _cacheService.GetAsync<User>($"user:{userId}", cancellationToken);
			if (cachedUser != null)
			{
				return ApiResponseDto<UserDto>.SuccessResponse(MapToUserDto(cachedUser));
			}

			var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);

			if (user == null || user.IsDeleted)
			{
				return ApiResponseDto<UserDto>.ErrorResponse(_localizer["UserNotFound"]);
			}

			// Cache the user
			await _cacheService.SetAsync($"user:{userId}", user, TimeSpan.FromHours(1), cancellationToken);

			return ApiResponseDto<UserDto>.SuccessResponse(MapToUserDto(user));
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting profile for user {UserId}", userId);
			return ApiResponseDto<UserDto>.ErrorResponse(_localizer["GetProfileFailed"]);
		}
	}

	public async Task<ApiResponseDto<UserDto>> UpdateProfileAsync(string userId, UpdateProfileDto request, CancellationToken cancellationToken = default)
	{
		try
		{
			var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);

			if (user == null || user.IsDeleted)
			{
				return ApiResponseDto<UserDto>.ErrorResponse(_localizer["UserNotFound"]);
			}

			// Track changes for audit
			var changes = new List<(string Field, object? OldValue, object? NewValue)>();

			if (!string.IsNullOrWhiteSpace(request.Name) && request.Name != user.Name)
			{
				changes.Add(("Name", user.Name, request.Name));
				user.Name = request.Name;
			}

			if (!string.IsNullOrWhiteSpace(request.PhoneNumber) && request.PhoneNumber != user.PhoneNumber)
			{
				// Check if phone number already exists
				if (await _unitOfWork.Users.FindOneAsync(new UserByPhoneNumberSpecification(request.PhoneNumber), cancellationToken) is not null)
				{
					//var existingUser = await _unitOfWork.Users.GetByPhoneAsync(request.PhoneNumber, cancellationToken);
					var existingUser = await _unitOfWork.Users.FindOneAsync(new UserByPhoneNumberSpecification(request.PhoneNumber), cancellationToken);

					if (existingUser?.Id != userId)
					{
						return ApiResponseDto<UserDto>.ErrorResponse(_localizer["PhoneNumberAlreadyExists"]);
					}
				}

				changes.Add(("PhoneNumber", user.PhoneNumber, request.PhoneNumber));
				user.PhoneNumber = request.PhoneNumber;
			}

			if (!string.IsNullOrWhiteSpace(request.Country) && request.Country != user.Country)
			{
				changes.Add(("Country", user.Country, request.Country));
				user.Country = request.Country;
			}

			if (!string.IsNullOrWhiteSpace(request.DisplayCalendar.ToString()) && request.DisplayCalendar != user.DisplayCalendar)
			{
				changes.Add(("DisplayCalendar", user.DisplayCalendar, request.DisplayCalendar));
				user.DisplayCalendar = request.DisplayCalendar;
			}

			if (!string.IsNullOrWhiteSpace(request.PreferredLanguage) && request.PreferredLanguage != user.PreferredLanguage)
			{
				changes.Add(("PreferredLanguage", user.PreferredLanguage, request.PreferredLanguage));
				user.PreferredLanguage = request.PreferredLanguage;
			}

			user.UpdatedAt = DateTime.UtcNow;
			await _unitOfWork.Users.UpdateAsync(user, cancellationToken);

			// Clear cache
			await _cacheService.RemoveAsync($"user:{userId}", cancellationToken);

			// Update search index

			_logger.LogInformation("Profile updated for user: {UserId}", userId);

			return ApiResponseDto<UserDto>.SuccessResponse(
				MapToUserDto(user),
				_localizer["ProfileUpdatedSuccessfully"]
			);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error updating profile for user {UserId}", userId);
			return ApiResponseDto<UserDto>.ErrorResponse(_localizer["UpdateProfileFailed"]);
		}
	}

	public async Task<ApiResponseDto<bool>> ChangePasswordAsync(string userId, ChangePasswordDto request, string ipAddress , CancellationToken cancellationToken = default)
	{
		try
		{
			var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);

			if (user == null || user.IsDeleted)
			{
				return ApiResponseDto<bool>.ErrorResponse(_localizer["UserNotFound"]);
			}

			// Verify current password
			if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
			{
				return ApiResponseDto<bool>.ErrorResponse(_localizer["InvalidCurrentPassword"]);
			}

			// Update password
			user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
			user.UpdatedAt = DateTime.UtcNow;
			await _unitOfWork.Users.UpdateAsync(user, cancellationToken);

			// Revoke all refresh tokens for security
			await _unitOfWork.Tokens.RevokeAllUserTokensAsync(userId, ipAddress, cancellationToken);

			// Clear cache
			await _cacheService.RemoveAsync($"user:{userId}", cancellationToken);

			_logger.LogInformation("Password changed for user: {UserId}", userId);

			return ApiResponseDto<bool>.SuccessResponse(true, _localizer["PasswordChangedSuccessfully"]);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error changing password for user {UserId}", userId);
			return ApiResponseDto<bool>.ErrorResponse(_localizer["PasswordChangeFailed"]);
		}
	}

	public async Task<ApiResponseDto<string>> UploadProfilePictureAsync(string userId, IFormFile file, CancellationToken cancellationToken = default)
	{
		try
		{
			var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);

			if (user == null || user.IsDeleted)
			{
				return ApiResponseDto<string>.ErrorResponse(_localizer["UserNotFound"]);
			}

			// Validate file
			var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
			var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

			if (!allowedExtensions.Contains(extension))
			{
				return ApiResponseDto<string>.ErrorResponse(_localizer["InvalidFileType"]);
			}

			if (file.Length > 5 * 1024 * 1024) // 5MB limit
			{
				return ApiResponseDto<string>.ErrorResponse(_localizer["FileTooLarge"]);
			}

			// Delete old profile picture if exists
			if (!string.IsNullOrEmpty(user.ProfilePictureUrl))
			{
				await _fileStorageService.DeleteFileAsync(user.ProfilePictureUrl, cancellationToken);
			}

			// Upload new profile picture
			using var stream = file.OpenReadStream();
			var fileName = $"{userId}_{DateTime.UtcNow:yyyyMMddHHmmss}{extension}";
			var fileUrl = await _fileStorageService.UploadFileAsync(
				stream,
				fileName,
				file.ContentType,
				"profile-pictures",
				cancellationToken
			);

			// Update user
			user.ProfilePictureUrl = fileUrl;
			user.UpdatedAt = DateTime.UtcNow;
			await _unitOfWork.Users.UpdateAsync(user, cancellationToken);

			// Clear cache
			await _cacheService.RemoveAsync($"user:{userId}", cancellationToken);

			// Update search index

			_logger.LogInformation("Profile picture uploaded for user: {UserId}", userId);

			return ApiResponseDto<string>.SuccessResponse(fileUrl, _localizer["ProfilePictureUploadedSuccessfully"]);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error uploading profile picture for user {UserId}", userId);
			return ApiResponseDto<string>.ErrorResponse(_localizer["ProfilePictureUploadFailed"]);
		}
	}

	public async Task<ApiResponseDto<bool>> DeleteAccountAsync(string userId, string ipAdress , DeleteAccountDto request, CancellationToken cancellationToken = default)
	{
		try
		{
			var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);

			if (user == null || user.IsDeleted)
			{
				return ApiResponseDto<bool>.ErrorResponse(_localizer["UserNotFound"]);
			}

			// Verify password
			if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
			{
				return ApiResponseDto<bool>.ErrorResponse(_localizer["InvalidPassword"]);
			}

			// Soft delete
			user.Status = UserStatus.Deleted;
			user.DeletedAt = DateTime.UtcNow;
			user.DeletionRequestedAt = DateTime.UtcNow;
			user.UpdatedAt = DateTime.UtcNow;
			await _unitOfWork.Users.UpdateAsync(user, cancellationToken);

			// Revoke all tokens
			await _unitOfWork.Tokens.RevokeAllUserTokensAsync(userId, ipAdress, cancellationToken);

			// Clear cache
			await _cacheService.RemoveAsync($"user:{userId}", cancellationToken);

			// Update search index (or remove)

			// Send deletion confirmation email
			await _emailService.SendAccountDeletionEmailAsync(user.Email, user.Name, cancellationToken);

			_logger.LogInformation("Account deletion requested for user: {UserId}, Reason: {Reason}",
				userId, request.Reason ?? "Not specified");

			return ApiResponseDto<bool>.SuccessResponse(
				true,
				_localizer["AccountDeletionScheduled"]
			);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error deleting account for user {UserId}", userId);
			return ApiResponseDto<bool>.ErrorResponse(_localizer["AccountDeletionFailed"]);
		}
	}

	public async Task<ApiResponseDto<bool>> RestoreAccountAsync(string userId, CancellationToken cancellationToken = default)
	{
		try
		{
			var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);

			if (user == null)
			{
				return ApiResponseDto<bool>.ErrorResponse(_localizer["UserNotFound"]);
			}

			if (!user.IsDeleted)
			{
				return ApiResponseDto<bool>.ErrorResponse(_localizer["AccountNotDeleted"]);
			}

			// Check if 30 days have passed (permanent deletion window)
			if (user.DeletionRequestedAt.HasValue &&
				user.DeletionRequestedAt.Value.AddDays(30) < DateTime.UtcNow)
			{
				return ApiResponseDto<bool>.ErrorResponse(_localizer["AccountCannotBeRestored"]);
			}

			// Restore account
			user.Status = UserStatus.Active;
			user.DeletedAt = null;
			user.DeletionRequestedAt = null;
			user.UpdatedAt = DateTime.UtcNow;
			await _unitOfWork.Users.UpdateAsync(user, cancellationToken);

			// Re-index in search

			_logger.LogInformation("Account restored for user: {UserId}", userId);

			return ApiResponseDto<bool>.SuccessResponse(true, _localizer["AccountRestoredSuccessfully"]);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error restoring account for user {UserId}", userId);
			return ApiResponseDto<bool>.ErrorResponse(_localizer["AccountRestoreFailed"]);
		}
	}

	public async Task<ApiResponseDto<bool>> PermanentDeleteAccountAsync(
		string userId,
		string ipAdress,
		string adminUserId,
		string reason,
		CancellationToken cancellationToken = default)
	{
		try
		{
			var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);

			if (user == null)
			{
				return ApiResponseDto<bool>.ErrorResponse(_localizer["UserNotFound"]);
			}

			// Log admin action
			await _auditService.LogAdminActionAsync(
				adminUserId,
				"USER_PERMANENT_DELETE",
				"users",
				userId,
				reason: reason,
				cancellationToken: cancellationToken
			);

			// Delete profile picture if exists
			if (!string.IsNullOrEmpty(user.ProfilePictureUrl))
			{
				await _fileStorageService.DeleteFileAsync(user.ProfilePictureUrl, cancellationToken);
			}

			// Permanently delete user
			await _unitOfWork.Users.DeleteAsync(userId, cancellationToken);

			// Delete all tokens
			await _unitOfWork.Tokens.RevokeAllUserTokensAsync(userId, ipAdress, cancellationToken);

			// Clear cache
			await _cacheService.RemoveAsync($"user:{userId}", cancellationToken);

			// Remove from search index

			_logger.LogWarning("User permanently deleted: {UserId} by admin: {AdminUserId}, Reason: {Reason}",
				userId, adminUserId, reason);

			return ApiResponseDto<bool>.SuccessResponse(true, _localizer["UserPermanentlyDeleted"]);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error permanently deleting user {UserId}", userId);
			return ApiResponseDto<bool>.ErrorResponse(_localizer["PermanentDeletionFailed"]);
		}
	}

	public async Task<ApiResponseDto<Dictionary<string, object>>> ShareProfileAsync(
		string userId,
		ShareProfileDto request,
		CancellationToken cancellationToken = default)
	{
		try
		{
			var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);

			if (user == null || user.IsDeleted)
			{
				return ApiResponseDto<Dictionary<string, object>>.ErrorResponse(_localizer["UserNotFound"]);
			}

			var sharedData = new Dictionary<string, object>
			{
				["id"] = user.Id,
				["name"] = user.Name,
				["profilePictureUrl"] = user.ProfilePictureUrl ?? string.Empty,
				["role"] = user.Role
			};

			if (request.IncludeEmail)
			{
				sharedData["email"] = user.Email;
			}

			if (request.IncludePhoneNumber)
			{
				sharedData["phoneNumber"] = user.PhoneNumber;
			}

			foreach (var field in request.IncludeFields)
			{
				var property = user.GetType().GetProperty(field);
				if (property != null)
				{
					sharedData[field] = property.GetValue(user) ?? string.Empty;
				}
			}

			_logger.LogInformation("Profile shared for user: {UserId}", userId);

			return ApiResponseDto<Dictionary<string, object>>.SuccessResponse(
				sharedData,
				_localizer["ProfileSharedSuccessfully"]
			);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error sharing profile for user {UserId}", userId);
			return ApiResponseDto<Dictionary<string, object>>.ErrorResponse(_localizer["ProfileShareFailed"]);
		}
	}

	public async Task<ApiResponseDto<PagedResultDto<UserDto>>> GetUsersAsync( UserParams userParams,
			CancellationToken cancellationToken = default)
	{
		try
		{

			var spec = new UsersSpecification(userParams);

			var (users, totalCount) = await _unitOfWork.Users.GetPagedAsync(
				spec,
				cancellationToken
			);

			var userDtos = users.Select(MapToUserDto).ToList();

			var result = new PagedResultDto<UserDto>
			{
				Items = userDtos,
				PageNumber = userParams.PageNumber,
				PageSize = userParams.PageSize,
				TotalCount = totalCount
			};

			return ApiResponseDto<PagedResultDto<UserDto>>.SuccessResponse(result);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting users list");
			return ApiResponseDto<PagedResultDto<UserDto>>.ErrorResponse(_localizer["GetUsersFailed"]);
		}
	}

	private UserDto MapToUserDto(User user)
	{
		return new UserDto
		{
			Id = user.Id,
			Name = user.Name,
			Email = user.Email,
			Country = user.Country,
			PhoneNumber = user.PhoneNumber,
			IsEmailVerified = user.IsEmailVerified,
			ProfilePictureUrl = user.ProfilePictureUrl,
			Role = user.Role,
			LastLogin = user.LastLogin,
			Status = user.Status.ToString(),
			DisplayCalendar = user.DisplayCalendar.ToString(),
			PreferredLanguage = user.PreferredLanguage,
			TwoFactorEnabled = user.TwoFactorEnabled,
			CreatedAt = user.CreatedAt
		};
	}

	
}