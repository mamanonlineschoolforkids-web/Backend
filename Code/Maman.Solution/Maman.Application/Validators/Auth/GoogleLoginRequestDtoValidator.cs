using FluentValidation;
using Maman.Application.DTOs.Auth;


namespace Maman.Application.Validators.Auth;

public class GoogleLoginRequestDtoValidator : AbstractValidator<GoogleLoginRequestDto>
{
	public GoogleLoginRequestDtoValidator()
	{
		RuleFor(x => x.IdToken)
			.NotEmpty().WithMessage("Google ID token is required");

		RuleFor(x => x.Country)
			.Length(2, 100).WithMessage("Country must be between 2 and 100 characters")
			.When(x => !string.IsNullOrEmpty(x.Country));

		RuleFor(x => x.PhoneNumber)
			.Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Invalid phone number format")
			.When(x => !string.IsNullOrEmpty(x.PhoneNumber));

		RuleFor(x => x.Role)
			.IsInEnum().WithMessage("Invalid role selected");
	}
}