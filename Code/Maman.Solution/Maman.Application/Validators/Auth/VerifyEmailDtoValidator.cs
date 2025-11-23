using FluentValidation;
using Maman.Application.DTOs.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maman.Application.Validators.Auth
{
	public class VerifyEmailDtoValidator : AbstractValidator<VerifyEmailDto>
	{
		public VerifyEmailDtoValidator()
		{
			RuleFor(x => x.Token)
				.NotEmpty().WithMessage("Verification token is required");
		}
	}
}
