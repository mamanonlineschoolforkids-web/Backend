using FluentValidation;
using Maman.Application.DTOs.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maman.Application.Validators.User
{
	public class ShareProfileDtoValidator : AbstractValidator<ShareProfileDto>
	{
		public ShareProfileDtoValidator()
		{
			RuleFor(x => x.IncludeFields)
				.Must(fields => fields == null || fields.All(f => !string.IsNullOrWhiteSpace(f)))
				.WithMessage("Field names cannot be empty or whitespace");
		}
	}
}
