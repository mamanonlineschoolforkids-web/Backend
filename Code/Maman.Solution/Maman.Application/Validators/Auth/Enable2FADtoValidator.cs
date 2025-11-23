using FluentValidation;
using Maman.Application.DTOs.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maman.Application.Validators.Auth
{
	public class Enable2FADtoValidator : AbstractValidator<Enable2FADto>
	{
		public Enable2FADtoValidator()
		{
			RuleFor(x => x.Password)
				.NotEmpty().WithMessage("Password is required");
		}
	}
}
