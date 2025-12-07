using FluentValidation;
using Maman.Application.DTOs.Auth;
using Maman.Localization;
using Microsoft.Extensions.Localization;

namespace Maman.Application.Validators.Auth;

public class LoginRequestDtoValidator : AbstractValidator<LoginRequestDto>
{
    private readonly IStringLocalizer<SharedResource> _localizer;

    public LoginRequestDtoValidator(IStringLocalizer<SharedResource> localizer)
	{
        _localizer = localizer;
        RuleFor(x => x.Email)
			.NotEmpty().WithMessage(_localizer["EmailIsRequired"])
			.EmailAddress().WithMessage(_localizer["InvalidEmailFormat"]);

		RuleFor(x => x.Password)
			.NotEmpty().WithMessage(_localizer["PasswordIsRequired"]);

		RuleFor(x => x.TwoFactorCode)
			.Length(6).WithMessage(_localizer["Two-factorCodeLength"])
			.Matches(@"^\d{6}$").WithMessage(_localizer["Two-factorCodeMustContainOnlyNumbers"])
			.When(x => !string.IsNullOrEmpty(x.TwoFactorCode));
      
    }
}