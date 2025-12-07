using Maman.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Maman.Application.Services.Utility;

public class CurrentUserService : ICurrentUserService
{
	private readonly IHttpContextAccessor _httpContextAccessor;

	public CurrentUserService(IHttpContextAccessor httpContextAccessor)
	{
		_httpContextAccessor = httpContextAccessor;
	}

	public string? UserId => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier).ToString();

	public string? Email => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email).ToString();

	public List<string> Roles => _httpContextAccessor.HttpContext?.User?
		.FindAll(ClaimTypes.Role)
		.Select(c => c.Value)
		.ToList() ?? new List<string>();

	public string? IpAddress => _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

	public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

}