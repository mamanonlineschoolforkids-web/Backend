using FluentValidation;
using Maman.Application.DTOs.Auth;
using Maman.Localization;
using Microsoft.Extensions.Localization;

namespace Maman.Application.Validators.Auth;

public class VerifyEmailDtoValidator : AbstractValidator<VerifyEmailDto>
{
	private readonly IStringLocalizer<SharedResource> _localizer;

	public VerifyEmailDtoValidator(IStringLocalizer<SharedResource> localizer)
	{
		_localizer=localizer;


		RuleFor(x => x.Token)
			.NotEmpty().WithMessage(_localizer["VerificaionTokenRequired"]);
	}
}
