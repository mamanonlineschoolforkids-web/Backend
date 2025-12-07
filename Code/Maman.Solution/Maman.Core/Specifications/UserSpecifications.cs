using Maman.Core.Entities.Auth;
using Maman.Core.Enums;

namespace Maman.Core.Specifications;

public class UsersSpecification : BaseSpecification<User>
{
	public UsersSpecification(UserParams userParams)
		: base(u =>
				 (string.IsNullOrEmpty(userParams.Search) || u.Name.ToLower().Contains(userParams.Search)) &&
				 (!userParams.Status.HasValue || u.Status == userParams.Status) &&
				 (!userParams.Role.HasValue || u.Role == userParams.Role) &&
				 (string.IsNullOrEmpty(userParams.Country)|| u.Country == userParams.Country)
				 )

	{
		ApplyPaging(userParams.PageNumber, userParams.PageSize);
		ApplySorting(userParams.Sort);
	}

	protected override void ApplySorting(string? sort)
	{
		
		if(sort is not null)
		{
			switch (sort)
			{
				case "name":
					ApplyOrderBy(p => p.Name);
					break;
				case "createdAtAsc":
					ApplyOrderBy(p => p.CreatedAt);
					break;
				case "createdAtDesc":
					ApplyOrderByDescending(p => p.CreatedAt);
					break;
				default:
					ApplyOrderBy(p => p.CreatedAt);
					break;
			}
		}
		else
		{
			ApplyOrderBy(p => p.CreatedAt);
		}
	}
}  
	

public class UserByEmailSpecification : BaseSpecification<User>
{
	public UserByEmailSpecification(string email)
		: base(u => u.Email == email)
	{
	}
}

public class UserByPhoneNumberSpecification : BaseSpecification<User>
{
	public UserByPhoneNumberSpecification(string phoneNumber)
		: base(u => u.PhoneNumber == phoneNumber )
	{
	}
}

public class UserByGoogleIdSpecification : BaseSpecification<User>
{
	public UserByGoogleIdSpecification(string googleId)
		: base(u => u.GoogleId == googleId )
	{
	}
}

public class UsersForPermanentDeletionSpecification : BaseSpecification<User>
{
	public UsersForPermanentDeletionSpecification(int daysAfterRequest)
		: base(u => u.Status == UserStatus.Deleted &&
			u.DeletionRequestedAt.HasValue &&
			u.DeletionRequestedAt.Value <= DateTime.UtcNow.AddDays(-daysAfterRequest))
	{
	
	}
}