using Maman.API.Exceptions;
using Maman.API.Extensions;
using System.Text.Json;

namespace Maman.API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);


		builder.Services.AddControllers().AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase); // Global camelCase;
		builder.Services.AddEndpointsApiExplorer();
		builder.Services.AddSwaggerGen();

		builder.Services.AddApplicationServices(builder.Configuration);

		var camelCaseConvention = new ConventionPack { new CamelCaseElementNameConvention() };
		ConventionRegistry.Register("CamelCase", camelCaseConvention, type => true);


		var app = builder.Build();

		#region DataSeeding

		#endregion

		#region Error Endpoints
		// Centralized endpoint for handling non-success status codes (e.g., 404)
		app.Map("/Error/{code:int}", (int code, ILogger<Program> logger) =>
		{
			logger.LogWarning("An error occurred with status code: {StatusCode}", code);
			return Results.Json(new BaseErrorResponse(code), statusCode: code);
		}).ExcludeFromDescription(); // Hide from Swagger


		// Centralized endpoint for unhandled exceptions (500)
		app.Map("/Error", (ILogger<Program> logger) =>
		{
			logger.LogError("An unhandled exception occurred.");
			return Results.Json(new BaseErrorResponse(500), statusCode: 500);
		}).ExcludeFromDescription(); 
		#endregion

		if (app.Environment.IsDevelopment())
		{
				app.UseSwagger();
				app.UseSwaggerUI();
		}

		app.UseExceptionHandler("/Error"); // Handles true exceptions (500-level)
		app.UseStatusCodePagesWithReExecute("/Error/{0}"); // Handles 404, 401, etc.


		app.UseHttpsRedirection();
		
		app.UseOutputCache();

		app.UseAuthorization();

		app.MapControllers();

		app.Run();
    }
}
