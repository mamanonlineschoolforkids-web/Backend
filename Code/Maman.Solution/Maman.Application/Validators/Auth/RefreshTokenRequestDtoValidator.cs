using FluentValidation;
using Maman.Application.DTOs.Auth;
using Maman.Localization;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maman.Application.Validators.Auth;

public class RefreshTokenRequestDtoValidator : AbstractValidator<RefreshTokenRequestDto>
{
    private readonly IStringLocalizer<SharedResource> _localizer;

    public RefreshTokenRequestDtoValidator(IStringLocalizer<SharedResource> localizer)
	{
        _localizer = localizer;
        RuleFor(x => x.RefreshToken)
			.NotEmpty().WithMessage(_localizer["RefreshTokenIsRequired"]);
       
    }
}
