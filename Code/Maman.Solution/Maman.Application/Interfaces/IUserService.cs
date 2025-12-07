using Maman.Application.DTOs.Auth;
using Maman.Application.DTOs.Common;
using Maman.Application.DTOs.User;
using Maman.Core.Enums;
using Maman.Core.Specifications;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maman.Application.Interfaces
{
	public interface IUserService
	{
		Task<ApiResponseDto<UserDto>> GetProfileAsync(string userId, CancellationToken cancellationToken = default);
		Task<ApiResponseDto<UserDto>> UpdateProfileAsync(string userId, UpdateProfileDto request, CancellationToken cancellationToken = default);
		Task<ApiResponseDto<bool>> ChangePasswordAsync(string userId, ChangePasswordDto request, string ipAddress, CancellationToken cancellationToken = default);
		Task<ApiResponseDto<string>> UploadProfilePictureAsync(string userId, IFormFile file, CancellationToken cancellationToken = default);
		Task<ApiResponseDto<bool>> DeleteAccountAsync(string userId, string ipAdress, DeleteAccountDto request, CancellationToken cancellationToken = default);
		Task<ApiResponseDto<bool>> RestoreAccountAsync(string userId, CancellationToken cancellationToken = default);
		Task<ApiResponseDto<bool>> PermanentDeleteAccountAsync(
		string userId,
		string adminUserId,
		string ipAdress,
		string reason,
		CancellationToken cancellationToken = default);
		Task<ApiResponseDto<Dictionary<string, object>>> ShareProfileAsync(
			string userId,
			ShareProfileDto request,
			CancellationToken cancellationToken = default);
		Task<ApiResponseDto<PagedResultDto<UserDto>>> GetUsersAsync(
			UserParams userParams,
			CancellationToken cancellationToken = default);
	}
}
