using FluentValidation;
using FluentValidation.AspNetCore;
using Maman.API.Filters;
using Maman.API.Middlewares;
using Maman.Application.Interfaces;
using Maman.Application.Services;
using Maman.Application.Services.Utility;
using Maman.Application.Validators.Auth;
using Maman.Core.Interfaces;
using Maman.Core.Settings;
using Maman.Infrastructure.Persistence;
using Maman.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;
using Serilog;
using Serilog.Events;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;

namespace Maman.API;

public class Program
{
	public static void Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);

		// Add Controllers with Filters
		builder.Services.AddControllers(options =>
		{
			options.Filters.Add<PerformanceFilter>();
		})
		.AddJsonOptions(options =>
		{
			options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
			options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
		});

		#region Serilog
		Log.Logger = new LoggerConfiguration()
			.MinimumLevel.Information()
			.MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
			.MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
			.Enrich.FromLogContext()
			.Enrich.WithProperty("Application", "Maman")
			.WriteTo.Console()
			.WriteTo.File(
				"logs/app-.log",
				rollingInterval: RollingInterval.Day,
				outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
			)
			.WriteTo.File(
				"logs/errors-.log",
				restrictedToMinimumLevel: LogEventLevel.Error,
				rollingInterval: RollingInterval.Day
			)
			.CreateLogger();

		builder.Host.UseSerilog();

		#endregion

		#region MongoDB

		var conventions = new ConventionPack {
			new CamelCaseElementNameConvention(),
			new EnumRepresentationConvention(BsonType.String),
			new IgnoreIfNullConvention(true)
		};
		ConventionRegistry.Register("CustomConventions", conventions, _ => true);

		builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDbSettings"));
		builder.Services.AddSingleton<MongoDbContext>();

		#endregion
		#region JWT

		builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
		var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();

		builder.Services.AddAuthentication(options =>
		{
			options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
			options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
		})
		.AddJwtBearer(options =>
		{
			options.TokenValidationParameters = new TokenValidationParameters
			{
				ValidateIssuerSigningKey = true,
				IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings!.SecretKey)),
				ValidateIssuer = true,
				ValidIssuer = jwtSettings.Issuer,
				ValidateAudience = true,
				ValidAudience = jwtSettings.Audience,
				ValidateLifetime = true,
				ClockSkew = TimeSpan.Zero
			};
		});

		#endregion


		#region Other settings

		builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
		builder.Services.Configure<GoogleAuthSettings>(builder.Configuration.GetSection("GoogleAuthSettings"));
		builder.Services.Configure<AzureStorageSettings>(builder.Configuration.GetSection("AzureStorageSettings"));
		builder.Services.Configure<AuthSettings>(builder.Configuration.GetSection("AuthSettings"));

		#endregion

		#region Caching (Hybrid: Memory + Redis)
	
		builder.Services.AddMemoryCache();
		builder.Services.AddStackExchangeRedisCache(options =>
		{
			options.Configuration = builder.Configuration.GetConnectionString("Redis");
			options.InstanceName = "Maman";
		});

		#endregion


		#region Repositories
		builder.Services.AddScoped<IUserRepository, UserRepository>();
		builder.Services.AddScoped<IFinanceAccountRepository, FinanceAccountRepository>();
		builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
		builder.Services.AddScoped<ITokenRepository, TokenRepository>();
		builder.Services.AddScoped<ITokenRepository, TokenRepository>(); 
		#endregion

		builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

		#region Services
		builder.Services.AddScoped<IAuthService, AuthService>();
		builder.Services.AddScoped<IUserService, UserService>();
		builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();
		builder.Services.AddScoped<IEmailService, EmailService>();
		builder.Services.AddScoped<IGoogleAuthService, GoogleAuthService>();
		builder.Services.AddSingleton<ITwoFactorService, TwoFactorService>();
		builder.Services.AddScoped<ICacheService, HybridCacheService>();
		builder.Services.AddScoped<IFileStorageService, FileStorageService>();
		builder.Services.AddScoped<IAuditService, AuditService>();
		builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

		#endregion

		// Add HttpContextAccessor
		builder.Services.AddHttpContextAccessor();

		#region Localization

		builder.Services.AddLocalization();
		builder.Services.Configure<RequestLocalizationOptions>(options =>
		{
			var supportedCultures = new[] { "en", "ar" };
			options.SetDefaultCulture("en")
				.AddSupportedCultures(supportedCultures)
				.AddSupportedUICultures(supportedCultures);

			options.RequestCultureProviders.Clear();
			options.RequestCultureProviders.Insert(0,
				new Microsoft.AspNetCore.Localization.AcceptLanguageHeaderRequestCultureProvider()
			);
		});
		#endregion

		#region  Rate Limiting
		builder.Services.AddRateLimiter(options =>
		{
			// Default fixed window limiter
			options.AddFixedWindowLimiter("fixed", opt =>
			{
				opt.PermitLimit = 100;
				opt.Window = TimeSpan.FromMinutes(1);
				opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
				opt.QueueLimit = 10;
			});

			// Stricter limiter for registration
			options.AddFixedWindowLimiter("register", opt =>
			{
				opt.PermitLimit = 5;
				opt.Window = TimeSpan.FromHours(1);
			});

			// Limiter for login attempts
			options.AddFixedWindowLimiter("login", opt =>
			{
				opt.PermitLimit = 10;
				opt.Window = TimeSpan.FromMinutes(15);
			});

			// Limiter for password reset
			options.AddFixedWindowLimiter("password-reset", opt =>
			{
				opt.PermitLimit = 3;
				opt.Window = TimeSpan.FromHours(1);
			});

			// Limiter for email verification
			options.AddFixedWindowLimiter("email-verification", opt =>
			{
				opt.PermitLimit = 5;
				opt.Window = TimeSpan.FromHours(1);
			});

			options.OnRejected = async (context, cancellationToken) =>
			{
				context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
				await context.HttpContext.Response.WriteAsJsonAsync(new
				{
					success = false,
					message = "Too many requests. Please try again later.",
					retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter)
						? retryAfter.ToString()
						: null
				}, cancellationToken);
			};
		});
		#endregion

		#region CORS
		builder.Services.AddCors(options =>
		{
			options.AddPolicy("AllowAll", corsBuilder =>
			{
				corsBuilder.WithOrigins("http://127.0.0.1:5500")
					   .AllowAnyMethod()
					   .AllowAnyHeader()
					   .AllowCredentials();
			});
		});
		#endregion

		#region Swagger
		builder.Services.AddEndpointsApiExplorer();
		builder.Services.AddSwaggerGen(c =>
		{
			c.SwaggerDoc("v1", new OpenApiInfo
			{
				Title = "Maman API",
				Version = "v1",
				Description = "API with Onion Architecture"
			});

			// Use inline definitions for enums
			c.UseInlineDefinitionsForEnums();

			// Add JWT Authentication to Swagger
			c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
			{
				Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token",
				Name = "Authorization",
				In = ParameterLocation.Header,
				Type = SecuritySchemeType.ApiKey,
				Scheme = "Bearer"
			});

			c.AddSecurityRequirement(new OpenApiSecurityRequirement
			{
				{
					new OpenApiSecurityScheme
					{
						Reference = new OpenApiReference
						{
							Type = ReferenceType.SecurityScheme,
							Id = "Bearer"
						}
					},
					Array.Empty<string>()
				}
			});
		});

		#endregion

		#region FluentValidation
		builder.Services.AddFluentValidationAutoValidation();
		builder.Services.AddFluentValidationClientsideAdapters();

		builder.Services.AddValidatorsFromAssemblyContaining<RegisterRequestDtoValidator>();

		builder.Services.AddScoped(typeof(ValidationFilter<>));
		#endregion



		#region Validation Error Handling

		builder.Services.Configure<ApiBehaviorOptions>(options =>
		{

			options.InvalidModelStateResponseFactory = (actionContext) =>
			{
				var errors = actionContext.ModelState.Where(p => p.Value.Errors.Count() > 0)
													.SelectMany(p => p.Value.Errors)
													.Select(e => e.ErrorMessage)
													.ToArray();

				var validationErrorResponse = new BaseErrorResponse(400, "A Validation error response occurred", null, errors);


				return new BadRequestObjectResult(validationErrorResponse);
			};
		});
		#endregion

		var app = builder.Build();

	

	
		app.UseSwagger();
		app.UseSwaggerUI();
		

		app.UseHttpsRedirection();

		#region Custom middleware (order matters!)
		app.UseRequestLogging();
		app.UseGlobalExceptionHandler();
		app.UsePerformanceLogging(); 
		#endregion

		app.UseCors("AllowAll");

		app.UseRequestLocalization();
		app.UseCustomLocalization();

		app.UseRateLimiter();

		app.UseAuthentication();
		app.UseAuthorization();

		app.MapControllers();

		// Log application start
		Log.Information("Application starting...");

		try
		{
			app.Run();
			Log.Information("Application stopped normally");
		}
		catch (Exception ex)
		{
			Log.Fatal(ex, "Application terminated unexpectedly");
		}
		finally
		{
			Log.CloseAndFlush();
		}
	}
}