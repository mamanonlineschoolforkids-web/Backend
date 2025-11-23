using Maman.Core.Enums;

namespace Maman.Core.Specifications;

public class UserParams
{
	public string? Sort { get; set; }
	public UserStatus? Status { get; set; }
	//public VerificationStatus? VerificationStatus { get; set; }

	public UserRole? Role { get; set; }
	public string? Country { get; set; }

	private string? search;
	public string? Search
	{
		get { return search; }
		set { search = value?.ToLower(); }
	}

	private const int MAX_PAGE_SIZE = 20;
	private int pageSize = MAX_PAGE_SIZE;
	public int PageSize
	{
		get { return pageSize; }
		set { pageSize = value > MAX_PAGE_SIZE ? MAX_PAGE_SIZE : value; }
	}
	public int PageNumber { get; set; } = 1;
}
