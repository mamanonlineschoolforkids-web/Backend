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

		RuleFor(x => x.Country)
			.Length(2, 100).WithMessage(_localizer["ValidationCountryLength"])
			.When(x => !string.IsNullOrEmpty(x.Country));

		RuleFor(x => x.PhoneNumber)
			.Matches(@"^\+?[1-9]\d{1,14}$").WithMessage(_localizer["InvalidPhoneNumberFormat"])
			.When(x => !string.IsNullOrEmpty(x.PhoneNumber));

		RuleFor(x => x.Role)
			.IsInEnum().WithMessage(_localizer["InvalidRoleSelected"]);
       
    }
}