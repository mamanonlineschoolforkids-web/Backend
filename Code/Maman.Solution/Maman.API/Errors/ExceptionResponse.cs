namespace Maman.API.Errors;

public class ExceptionResponse : BaseErrorResponse
{

	/// Use Cases
	/// Internal Server Error
	/// Create a Middleware to handle 
	public string Details { get; set; }

	public ExceptionResponse(int statusCode, string message = null, string details = null)
		: base(statusCode, message)
	{
		Details = details;
	}
}

