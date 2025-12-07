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
	public class Disable2FADtoValidator : AbstractValidator<Disable2FADto>
	{
        private readonly IStringLocalizer<SharedResource> _localizer;

        public Disable2FADtoValidator(IStringLocalizer<SharedResource> localizer )
		{
            _localizer = localizer;
            RuleFor(x => x.Code)
				.NotEmpty().WithMessage(_localizer["VerificationCodeIsRequired"])
				.Length(6).WithMessage(_localizer["CodeMustBe6Digits"])
				.Matches(@"^\d{6}$").WithMessage(_localizer["CodeMustContainOnlyNumbers"]);
           
        }
	}
}
