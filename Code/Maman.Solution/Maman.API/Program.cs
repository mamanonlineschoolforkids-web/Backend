using Maman.API.Errors;
using Maman.API.Extensions;
using Maman.API.Handlers;
using Maman.API.Middlewares;
using Maman.Infrastructure.Data;
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


		builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
		var app = builder.Build();

		app.Lifetime.ApplicationStopping.Register(() =>
		{
			// Dispose the context if you have access to the service provider
			using var scope = app.Services.CreateScope();
			if (scope.ServiceProvider.GetService<MongoDbContext>() is IDisposable disposable)
			{
				disposable.Dispose();
			}
		});

		#region DataSeeding

		#endregion

		#region ExceptionHandling
		app.UseMiddleware<ExceptionHandlerMiddleware>();
		#endregion

		if (app.Environment.IsDevelopment())
			{
				app.UseSwagger();
				app.UseSwaggerUI();
			}

		if (!app.Environment.IsDevelopment())
		{
			app.UseExceptionHandler();
			app.UseStatusCodePagesWithReExecute("/Error/{0}");
		}
		else
		{
			app.UseDeveloperExceptionPage();
		}

		// Add this endpoint to catch 404s etc.
		app.Map("/Error/{code:int}", (int code) =>
		{
			var logger = app.Services.GetRequiredService<ILogger<Program>>();
			logger.LogWarning("Endpoint not found: {Code}", code);

			var response = new BaseErrorResponse(code);
			return Results.Json(response, statusCode: code, options: new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
		}).ExcludeFromDescription(); // Hide from Swagger

		app.UseHttpsRedirection();

		app.UseAuthorization();

		app.MapControllers();

		app.Run();
    }
}
