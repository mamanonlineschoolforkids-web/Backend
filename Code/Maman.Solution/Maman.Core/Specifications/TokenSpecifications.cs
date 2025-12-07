using Maman.Core.Entities.Tokens;
using Maman.Core.Enums;

namespace Maman.Core.Specifications;

public class ValidTokenSpecifications : BaseSpecification<Token>
{
	public ValidTokenSpecifications(string token , TokenType tokenType) : 
		base(t => t.UserToken == token && t.TokenType == tokenType && t.ExpiresAt > DateTime.UtcNow && t.IsDeleted == false)
	{
	}
}
