using FluentValidation;
using Maman.Application.DTOs.Auth;
using Maman.Localization;
using Microsoft.Extensions.Localization;

namespace Maman.Application.Validators.Auth;

public class RegisterRequestDtoValidator : AbstractValidator<RegisterRequestDto>
{
    private readonly IStringLocalizer<SharedResource> _localizer;

    public RegisterRequestDtoValidator(IStringLocalizer<SharedResource> localizer)
	{
        _localizer = localizer;
        RuleFor(x => x.Name)
			.NotEmpty().WithMessage(_localizer["NameIsRequired"])
			.Length(2, 100).WithMessage(_localizer["ValidationNameLength"])
			.Matches(@"^[a-zA-Z\s]+$").WithMessage(_localizer["ValidationNameAllowedCharacters"]);

		RuleFor(x => x.Email)
			.NotEmpty().WithMessage(_localizer["EmailIsRequired"])
			.EmailAddress().WithMessage(_localizer["InvalidEmailFormat"])
			.MaximumLength(255).WithMessage(_localizer["validationEmailMaxLength"]);

		RuleFor(x => x.Password)
			.NotEmpty().WithMessage(_localizer["PasswordIsRequired"])
			.MinimumLength(8).WithMessage(_localizer["PasswordMinLength"])
			.MaximumLength(100).WithMessage(_localizer["PasswordMaxLength"])
			.Matches(@"[A-Z]").WithMessage(_localizer["ValidationPasswordUppercaseRequired"])
			.Matches(@"[a-z]").WithMessage(_localizer["ValidationPasswordLowercaseRequired"])
			.Matches(@"[0-9]").WithMessage(_localizer["Password must contain at least one number"])
			.Matches(@"[@$!%*?&#]").WithMessage(_localizer["ValidationPasswordSpecialCharRequired"]);

		RuleFor(x => x.ConfirmPassword)
			.NotEmpty().WithMessage(_localizer["PasswordConfirmationIsRequired"])
			.Equal(x => x.Password).WithMessage(_localizer["PasswordsMismatch"]);

		RuleFor(x => x.PhoneNumber)
			.NotEmpty().WithMessage(_localizer["PhoneNumberIsRequired"])
			.Matches(@"^\+?[1-9]\d{1,14}$").WithMessage(_localizer["InvalidPhoneNumberFormat"]);

		RuleFor(x => x.Country)
			.NotEmpty().WithMessage(_localizer["CountryIsRequired"])
			.Length(2, 100).WithMessage(_localizer["ValidationCountryLength"]);

		RuleFor(x => x.Role)
			.NotNull().WithMessage(_localizer["RoleIsRequired"])
			.IsInEnum().WithMessage(_localizer["InvalidRoleSelected"]);
       
    }
}
