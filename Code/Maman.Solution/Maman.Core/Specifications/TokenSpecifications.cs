using Maman.Core.Entities.Tokens;

namespace Maman.Core.Specifications;

public class ValidEmailTokenSpecifications : BaseSpecification<EmailVerificationToken>
{
	public ValidEmailTokenSpecifications(string token) : 
		base(t => t.Token == token && t.ExpiresAt > DateTime.UtcNow && t.IsUsed == false)
	{
	}
}

public class ValidPasswordResetTokenSpecifications : BaseSpecification<PasswordResetToken>
{
	public ValidPasswordResetTokenSpecifications(string token) :
		base(t => t.Token == token && t.ExpiresAt > DateTime.UtcNow && t.IsUsed == false)
	{
	}
}