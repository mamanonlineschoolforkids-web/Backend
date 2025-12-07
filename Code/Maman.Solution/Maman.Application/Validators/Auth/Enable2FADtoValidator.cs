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
	public class Enable2FADtoValidator : AbstractValidator<Enable2FADto>
	{
        private readonly IStringLocalizer<SharedResource> _localizer;

        public Enable2FADtoValidator(IStringLocalizer<SharedResource> localizer)
		{
            _localizer = localizer;
            RuleFor(x => x.Password)
				.NotEmpty().WithMessage(_localizer["PasswordIsRequired"]);
          
        }
	}
}
