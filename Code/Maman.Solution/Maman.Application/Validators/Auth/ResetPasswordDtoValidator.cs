using FluentValidation;
using Maman.Application.DTOs.Auth;
using Maman.Localization;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maman.Application.Validators.Auth
{
	public class ResetPasswordDtoValidator : AbstractValidator<ResetPasswordDto>
	{
        private readonly IStringLocalizer<SharedResource> _localizer;

        public ResetPasswordDtoValidator(IStringLocalizer<SharedResource> localizer)
		{
            _localizer = localizer;
            RuleFor(x => x.Token)
				.NotEmpty().WithMessage(_localizer["ResetTokenIsRequired"]);

			RuleFor(x => x.NewPassword)
				.NotEmpty().WithMessage(_localizer["NewPasswordIsRequired"])
				.MinimumLength(8).WithMessage(_localizer["PasswordMinLength"])
				.MaximumLength(100).WithMessage(_localizer["PasswordMaxLength"])
				.Matches(@"[A-Z]").WithMessage(_localizer["ValidationPasswordUppercaseRequired"])
				.Matches(@"[a-z]").WithMessage(_localizer["ValidationPasswordLowercaseRequired"])
				.Matches(@"[0-9]").WithMessage(_localizer["ValidationPasswordNumberRequired"])
				.Matches(@"[@$!%*?&#]").WithMessage(_localizer["ValidationPasswordSpecialCharRequired"]);


            RuleFor(x => x.ConfirmPassword)
				.NotEmpty().WithMessage(_localizer["PasswordConfirmationIsRequired"])
				.Equal(x => x.NewPassword).WithMessage(_localizer["PasswordsMismatch"]);
           
        }
	}
}
