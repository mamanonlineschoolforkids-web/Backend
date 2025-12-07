using Microsoft.Extensions.Localization;

namespace Maman.Application.DTOs.Common;

public class ApiResponseDto<T>
{
	public bool Success { get; set; }
	public string Message { get; set; }
	public List<string> Errors { get; set; }
	public T? Data { get; set; }


	internal static ApiResponseDto<T> ErrorResponse(LocalizedString localizedString)
	{
		return new ApiResponseDto<T>
		{
			Success = false,
			Message = localizedString.Value,
			Errors = new List<string> { localizedString.Value },
			Data = default
		};
	}

	internal static ApiResponseDto<T> SuccessResponse(T value, LocalizedString localizedString)
	{
		return new ApiResponseDto<T>
		{
			Success = true,
			Message = localizedString.Value,
			Data = value,
			Errors = new()
		};
	}
	internal static ApiResponseDto<T> SuccessResponse(T value)
	{
		return new ApiResponseDto<T>
		{
			Success = true,
			Message = "Success",
			Data = value,
			Errors = new()
		};
	}
}
