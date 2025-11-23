using FluentValidation;
using Maman.Application.DTOs.Auth;

namespace Maman.Application.Validators.Auth;

public class LoginRequestDtoValidator : AbstractValidator<LoginRequestDto>
{
	public LoginRequestDtoValidator()
	{
		RuleFor(x => x.Email)
			.NotEmpty().WithMessage("Email is required")
			.EmailAddress().WithMessage("Invalid email format");

		RuleFor(x => x.Password)
			.NotEmpty().WithMessage("Password is required");

		RuleFor(x => x.TwoFactorCode)
			.Length(6).WithMessage("Two-factor code must be 6 digits")
			.Matches(@"^\d{6}$").WithMessage("Two-factor code must contain only numbers")
			.When(x => !string.IsNullOrEmpty(x.TwoFactorCode));
	}
}