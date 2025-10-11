namespace Maman.API.Errors;

public class ValidationErrorResponse: BaseErrorResponse
{
	/// Use Cases
	/// Wrong Parameters Passed   [ Handled in Main , The end point isn't Executed ]
	/// change the ApiBehaviorOptions service configurations
	public IEnumerable<string> Errors { get; set; }

	public ValidationErrorResponse() : base(400)
	{
		Errors = new List<string>();
	}
}