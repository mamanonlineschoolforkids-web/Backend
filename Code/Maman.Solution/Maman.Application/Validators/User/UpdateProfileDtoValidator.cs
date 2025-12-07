using FluentValidation;
using Maman.Application.DTOs.User;
using Maman.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maman.Application.Validators.User
{
	public class UpdateProfileDtoValidator : AbstractValidator<UpdateProfileDto>
	{
		public UpdateProfileDtoValidator()
		{
			RuleFor(x => x.Name)
				.Length(2, 100).WithMessage("Name must be between 2 and 100 characters")
				.Matches(@"^[a-zA-Z\s]+$").WithMessage("Name can only contain letters and spaces")
				.When(x => !string.IsNullOrEmpty(x.Name));

			RuleFor(x => x.PhoneNumber)
				.Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Invalid phone number format")
				.When(x => !string.IsNullOrEmpty(x.PhoneNumber));

			RuleFor(x => x.Country)
				.Length(2, 100).WithMessage("Country must be between 2 and 100 characters")
				.When(x => !string.IsNullOrEmpty(x.Country));

			RuleFor(x => x.DisplayCalendar)
				.Must(cal => cal == CalendarType.Hijri || cal == CalendarType.Gregorian)
				.WithMessage("Display calendar must be either 'gregorian' or 'hijri'")
				.When(x => !string.IsNullOrEmpty(x.DisplayCalendar.ToString()));

			RuleFor(x => x.PreferredLanguage)
				.Must(lang => lang == PreferredLanguage.Ar || lang == PreferredLanguage.En)
				.WithMessage("Preferred language must be either 'en-US' or 'ar-EG'");
				
		}
	}

}
