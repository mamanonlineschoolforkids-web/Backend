namespace Maman.API.Helpers;

public class ValidationFilter<T> : IAsyncActionFilter where T : class
{
	private readonly IValidator<T> _validator;

	public ValidationFilter(IValidator<T> validator)
	{
		_validator = validator;
	}

	public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
	{
		var argument = context.ActionArguments.FirstOrDefault(kvp => kvp.Value is T).Value as T;

		if (argument != null)
		{
			var validationResult = await _validator.ValidateAsync(argument);

			if (!validationResult.IsValid)
			{
				// Instead of creating a response, add the errors to the official ModelState.
				foreach (var error in validationResult.Errors)
				{
					context.ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
				}
			}
		}

		if (!context.ModelState.IsValid)
		{
			// This includes both model binding errors and your FluentValidation errors.
			var errors = context.ModelState
								.Where(p => p.Value.Errors.Any())
								.SelectMany(p => p.Value.Errors)
								.Select(e => e.ErrorMessage)
								.ToArray();

			// 2. Create your custom response object.
			var errorResponse = new BaseErrorResponse(400, "A Validation error occurred", null, errors);

			context.Result = new BadRequestObjectResult(errorResponse);
			return; // Stop the pipeline
		}

		await next();
	}
}