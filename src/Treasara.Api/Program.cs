using Asp.Versioning;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Treasara.Api.Configuration;
using Treasara.Api.Dtos.Requests;
using Treasara.Api.Dtos.Responses;
using Treasara.Api.Exceptions;
using Treasara.Api.Health;
using Treasara.Api.Mapping.Requests;
using Treasara.Api.Middleware;
using Treasara.Api.Validators;

var builder = WebApplication.CreateBuilder(args);

// Configure services
builder.Services.AddControllers();

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
})
.AddMvc()
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'V";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.Configure<RateLimitingOptions>(
    builder.Configuration.GetSection("RateLimiting"));

var rateLimitConfig = builder.Configuration
    .GetSection("RateLimiting")
    .Get<RateLimitingOptions>();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddFixedWindowLimiter("public-api", limiterOptions =>
    {
        limiterOptions.PermitLimit = rateLimitConfig?.PermitLimit ?? 30;
        limiterOptions.Window = TimeSpan.FromSeconds(rateLimitConfig?.WindowSeconds ?? 60);
        limiterOptions.QueueLimit = rateLimitConfig?.QueueLimit ?? 0;
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(cfg => { }, typeof(Program));

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddScoped<IBondRequestMapper, BondRequestMapper>();
builder.Services.AddScoped<IValidationErrorResponseFactory, ValidationErrorResponseFactory>();
builder.Services.AddHealthChecks();

// Register validators
ConfigureValidators(builder.Services);

var app = builder.Build();

// Configure middleware pipeline
app.UseExceptionHandler(new ExceptionHandlerOptions
{
    // In .NET 10, handled exceptions suppress diagnostics by default.
    // Set this to false to keep logs/metrics for handled exceptions.
    SuppressDiagnosticsCallback = _ => false
});

app.UseSwagger();
app.UseSwaggerUI();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Remove noisy headers
var securityHeadersSection = builder.Configuration.GetSection("SecurityHeaders");
var securityHeadersOptions = securityHeadersSection.Get<SecurityHeadersOptions>();
app.Use(async (context, next) =>
{
    context.Response.OnStarting(() =>
    {
        if (securityHeadersOptions?.Remove != null)
        {
            foreach (var header in securityHeadersOptions.Remove)
            {
                context.Response.Headers.Remove(header);
            }
        }

        return Task.CompletedTask;
    });

    await next();
});

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    ResponseWriter = HealthCheckResponseWriter.WriteResponse
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    ResponseWriter = HealthCheckResponseWriter.WriteResponse
});

app.UseRateLimiter();
app.UseRequestLogging();
app.UseCorrelationId();
app.MapControllers().RequireRateLimiting("public-api");
app.Run();

/// <summary>
/// Registers all FluentValidation validators with the dependency injection container.
/// </summary>
/// <param name="services">The service collection to register validators with.</param>
/// <remarks>
/// Validators are registered with scoped lifetime to align with the HTTP request lifecycle.
/// Add new validator registrations here as the API expands to support additional endpoints.
/// </remarks>
static void ConfigureValidators(IServiceCollection services)
{
    services.AddScoped<IValidator<BondValuationRequestDto>, BondValuationRequestValidator>();    
}

public partial class Program
{
}