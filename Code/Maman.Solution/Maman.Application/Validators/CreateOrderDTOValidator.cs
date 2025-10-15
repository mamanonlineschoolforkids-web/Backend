namespace Maman.Application.Validators;

public class CreateOrderDTOValidator : AbstractValidator<CreateOrderDTO>
{
	public CreateOrderDTOValidator()
	{
		RuleFor(x => x.ProductId)
			.Length(1,2).WithMessage("ProductId is short.");
	}
}
