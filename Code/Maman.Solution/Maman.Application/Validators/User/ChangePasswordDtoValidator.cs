using FluentValidation;
using Maman.Application.DTOs.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maman.Application.Validators.User
{
	public class ChangePasswordDtoValidator : AbstractValidator<ChangePasswordDto>
	{
		public ChangePasswordDtoValidator()
		{
			RuleFor(x => x.CurrentPassword)
				.NotEmpty().WithMessage("Current password is required");

			RuleFor(x => x.NewPassword)
				.NotEmpty().WithMessage("New password is required")
				.MinimumLength(8).WithMessage("Password must be at least 8 characters")
				.MaximumLength(100).WithMessage("Password must not exceed 100 characters")
				.Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter")
				.Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter")
				.Matches(@"[0-9]").WithMessage("Password must contain at least one number")
				.Matches(@"[@$!%*?&#]").WithMessage("Password must contain at least one special character")
				.NotEqual(x => x.CurrentPassword).WithMessage("New password must be different from current password");

			RuleFor(x => x.ConfirmPassword)
				.NotEmpty().WithMessage("Password confirmation is required")
				.Equal(x => x.NewPassword).WithMessage("Passwords do not match");
		}
	}

}
