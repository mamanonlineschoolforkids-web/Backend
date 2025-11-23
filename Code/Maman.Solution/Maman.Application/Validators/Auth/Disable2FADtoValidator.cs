using FluentValidation;
using Maman.Application.DTOs.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maman.Application.Validators.Auth
{
	public class Disable2FADtoValidator : AbstractValidator<Disable2FADto>
	{
		public Disable2FADtoValidator()
		{
			RuleFor(x => x.Code)
				.NotEmpty().WithMessage("Verification code is required")
				.Length(6).WithMessage("Code must be 6 digits")
				.Matches(@"^\d{6}$").WithMessage("Code must contain only numbers");
		}
	}
}
