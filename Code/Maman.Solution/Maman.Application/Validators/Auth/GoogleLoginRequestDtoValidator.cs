using FluentValidation;
using Maman.Application.DTOs.Auth;
using Maman.Localization;
using Microsoft.Extensions.Localization;


namespace Maman.Application.Validators.Auth;

public class GoogleLoginRequestDtoValidator : AbstractValidator<GoogleLoginRequestDto>
{
    private readonly IStringLocalizer<SharedResource> _localizer;

    public GoogleLoginRequestDtoValidator(IStringLocalizer<SharedResource> localizer)
	{
        _localizer = localizer;
        RuleFor(x => x.IdToken)
			.NotEmpty().WithMessage(_localizer["GoogleIDTokenIsRequired"]);
    }
}