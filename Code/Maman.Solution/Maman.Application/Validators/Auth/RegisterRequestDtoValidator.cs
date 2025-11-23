using FluentValidation;
using Maman.Application.DTOs.Auth;

namespace Maman.Application.Validators.Auth;

public class RegisterRequestDtoValidator : AbstractValidator<RegisterRequestDto>
{
	public RegisterRequestDtoValidator()
	{
		RuleFor(x => x.Name)
			.NotEmpty().WithMessage("Name is required")
			.Length(2, 100).WithMessage("Name must be between 2 and 100 characters")
			.Matches(@"^[a-zA-Z\s]+$").WithMessage("Name can only contain letters and spaces");

		RuleFor(x => x.Email)
			.NotEmpty().WithMessage("Email is required")
			.EmailAddress().WithMessage("Invalid email format")
			.MaximumLength(255).WithMessage("Email must not exceed 255 characters");

		RuleFor(x => x.Password)
			.NotEmpty().WithMessage("Password is required")
			.MinimumLength(8).WithMessage("Password must be at least 8 characters")
			.MaximumLength(100).WithMessage("Password must not exceed 100 characters")
			.Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter")
			.Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter")
			.Matches(@"[0-9]").WithMessage("Password must contain at least one number")
			.Matches(@"[@$!%*?&#]").WithMessage("Password must contain at least one special character (@$!%*?&#)");

		RuleFor(x => x.ConfirmPassword)
			.NotEmpty().WithMessage("Password confirmation is required")
			.Equal(x => x.Password).WithMessage("Passwords do not match");

		RuleFor(x => x.PhoneNumber)
			.NotEmpty().WithMessage("Phone number is required")
			.Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Invalid phone number format (E.164 format expected)");

		RuleFor(x => x.Country)
			.NotEmpty().WithMessage("Country is required")
			.Length(2, 100).WithMessage("Country must be between 2 and 100 characters");

		RuleFor(x => x.Role)
			.NotNull().WithMessage("Role is required")
			.IsInEnum().WithMessage("Invalid role selected");

		RuleFor(x => x.PreferredLanguage)
			.NotEmpty().WithMessage("Preferred language is required")
			.Must(lang => lang == "en-US" || lang == "ar-EG")
			.WithMessage("Preferred language must be either 'en-US' or 'ar-EG'");
	}
}
