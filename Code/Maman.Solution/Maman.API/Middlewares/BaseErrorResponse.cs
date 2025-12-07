using System.Text.Json.Serialization;

namespace Maman.API.Middlewares;

public class BaseErrorResponse
{
    [JsonPropertyName("statusCode")]
    public int StatusCode { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("details")]
    public string? Details { get; set; } // For dev/prod diffs (e.g., stack trace)

    [JsonPropertyName("errors")]
    public IEnumerable<string>? Errors { get; set; } // validation

	public BaseErrorResponse(int statusCode, string message = null, string? details = null, IEnumerable<string>? errors = null)
    {
        StatusCode = statusCode;
        Message = message ?? GetStatusCodeDefaultMessage(statusCode);
        Details = details;
        Errors = errors;
    }

    private static string GetStatusCodeDefaultMessage(int statusCode) => statusCode switch
    {
        400 => "Bad Request",
        401 => "Unauthorized",
        403 => "Forbidden",
        404 => "Not Found",
        500 => "Internal Server Error",
        _ => "An unexpected error occurred"
    };
}