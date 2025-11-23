using FluentValidation;
using Maman.Application.DTOs.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maman.Application.Validators.Auth
{
	public class RequestPasswordResetDtoValidator : AbstractValidator<RequestPasswordResetDto>
	{
		public RequestPasswordResetDtoValidator()
		{
			RuleFor(x => x.Email)
				.NotEmpty().WithMessage("Email is required")
				.EmailAddress().WithMessage("Invalid email format");
		}
	}
}
