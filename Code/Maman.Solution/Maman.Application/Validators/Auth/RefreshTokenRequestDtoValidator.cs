using FluentValidation;
using Maman.Application.DTOs.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maman.Application.Validators.Auth;

public class RefreshTokenRequestDtoValidator : AbstractValidator<RefreshTokenRequestDto>
{
	public RefreshTokenRequestDtoValidator()
	{
		RuleFor(x => x.RefreshToken)
			.NotEmpty().WithMessage("Refresh token is required");
	}
}
