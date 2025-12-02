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
	public class RequestPasswordResetDtoValidator : AbstractValidator<RequestPasswordResetDto>
	{
        private readonly IStringLocalizer<SharedResource> _localizer;

        public RequestPasswordResetDtoValidator(IStringLocalizer<SharedResource> localizer)
		{
			_localizer = localizer;
			RuleFor(x => x.Email)
				.NotEmpty().WithMessage(_localizer["EmailIsRequired"])
				.EmailAddress().WithMessage(_localizer["InvalidEmailFormat"]);
          
        }
	}
}
