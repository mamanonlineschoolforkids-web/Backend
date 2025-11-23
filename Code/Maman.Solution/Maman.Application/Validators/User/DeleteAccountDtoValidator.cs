using FluentValidation;
using Maman.Application.DTOs.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maman.Application.Validators.User
{
	public class DeleteAccountDtoValidator : AbstractValidator<DeleteAccountDto>
	{
		public DeleteAccountDtoValidator()
		{
			RuleFor(x => x.Password)
				.NotEmpty().WithMessage("Password is required for account deletion");

			RuleFor(x => x.Reason)
				.MaximumLength(500).WithMessage("Reason must not exceed 500 characters")
				.When(x => !string.IsNullOrEmpty(x.Reason));
		}
	}
}
