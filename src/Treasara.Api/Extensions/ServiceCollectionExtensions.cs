using Asp.Versioning;
using FluentValidation;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Treasara.Api.Configuration;
using Treasara.Api.Dtos.Requests;
using Treasara.Api.Exceptions;
using Treasara.Api.Mapping.Requests;
using Treasara.Api.Validators;
using Treasara.Api.Dtos.Responses;

namespace Treasara.Api.Extensions;

/// <summary>
/// Extension methods for configuring application services in the dependency injection container.
/// </summary>
/// <remarks>
/// This class provides extension methods for registering all application services, including:
/// <list type="bullet">
/// <item><description>Controllers and MVC services</description></item>
/// <item><description>API versioning configuration</description></item>
/// <item><description>Rate limiting policies</description></item>
/// <item><description>OpenAPI/Swagger documentation</description></item>
/// <item><description>Application-specific services (mappers, validators, exception handlers)</description></item>
/// <item><description>Health check services</description></item>
/// <item><description>CORS policies</description></item>
/// <item><description>FluentValidation validators</description></item>
/// </list>
/// 
/// The extension method pattern provides several benefits:
/// <list type="bullet">
/// <item><description>Organizes related service registrations into logical groups</description></item>
/// <item><description>Keeps Program.cs clean and maintainable</description></item>
/// <item><description>Makes it easy to find and modify specific configuration areas</description></item>
/// <item><description>Enables reusability across different applications or test scenarios</description></item>
/// </list>
/// 
/// Service registration follows these principles:
/// <list type="bullet">
/// <item><description>Configuration-driven setup using appsettings.json</description></item>
/// <item><description>Appropriate service lifetimes (Singleton, Scoped, Transient)</description></item>
/// <item><description>Fail-fast with sensible defaults when configuration is missing</description></item>
/// <item><description>Clear separation between infrastructure and application services</description></item>
/// </list>
/// </remarks>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// The name of the CORS policy used for UI/frontend applications.
    /// </summary>
    /// <remarks>
    /// This policy name is referenced in the middleware pipeline to enable CORS for specific origins.
    /// The actual allowed origins are configured in appsettings.json under "CORS:AllowedOrigins".
    /// </remarks>
    public const string CorsPolicyName = "UiCors";

    /// <summary>
    /// The name of the rate limiting policy applied to public API endpoints.
    /// </summary>
    /// <remarks>
    /// This policy name is used when mapping endpoints with rate limiting requirements.
    /// The actual rate limits are configured in appsettings.json under "RateLimiting".
    /// </remarks>
    public const string PublicApiRateLimitPolicy = "public-api";

    /// <summary>
    /// Registers all services required by the Treasara API application.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configuration">The application configuration containing settings from appsettings.json.</param>
    /// <returns>The service collection for method chaining.</returns>
    /// <remarks>
    /// This is the main orchestration method that calls all other service registration methods
    /// in the appropriate order. It ensures that all dependencies are registered before they're needed.
    /// 
    /// Services are registered in the following order:
    /// <list type="number">
    /// <item><description>Controllers - ASP.NET Core MVC controllers</description></item>
    /// <item><description>API Versioning - URL-based versioning support</description></item>
    /// <item><description>Rate Limiting - Throttling policies for API protection</description></item>
    /// <item><description>OpenAPI - Swagger documentation generation</description></item>
    /// <item><description>Application Services - Domain-specific services and handlers</description></item>
    /// <item><description>Health Checks - Liveness and readiness probes</description></item>
    /// <item><description>CORS - Cross-origin resource sharing policies</description></item>
    /// <item><description>Validators - FluentValidation validators for request DTOs</description></item>
    /// </list>
    /// 
    /// This method is called from Program.cs:
    /// <code>
    /// builder.Services.AddTreasaraApi(builder.Configuration);
    /// </code>
    /// </remarks>
    public static IServiceCollection AddTreasaraApi(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register ASP.NET Core MVC controllers
        services.AddControllers();

        // Register organized service groups
        services.AddTreasaraApiVersioning();
        services.AddTreasaraRateLimiting(configuration);
        services.AddTreasaraOpenApi();
        services.AddTreasaraApplicationServices();
        services.AddTreasaraHealthChecks();
        services.AddTreasaraCors(configuration);
        services.AddTreasaraValidators();

        return services;
    }

    /// <summary>
    /// Configures API versioning using URL-based versioning (e.g., /api/v1/..., /api/v2/...).
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The service collection for method chaining.</returns>
    /// <remarks>
    /// API versioning configuration:
    /// <list type="bullet">
    /// <item><description><b>Default version</b>: 1.0 - Used when clients don't specify a version</description></item>
    /// <item><description><b>Version reader</b>: UrlSegmentApiVersionReader - Reads version from URL path</description></item>
    /// <item><description><b>ReportApiVersions</b>: true - Adds API version headers to responses</description></item>
    /// <item><description><b>API Explorer</b>: Configured for Swagger integration with version substitution</description></item>
    /// </list>
    /// 
    /// URL format: <c>/api/v{version}/[controller]/[action]</c>
    /// 
    /// Examples:
    /// <list type="bullet">
    /// <item><description>/api/v1/bonds/value - Version 1.0 endpoint</description></item>
    /// <item><description>/api/v2/bonds/value - Version 2.0 endpoint (future)</description></item>
    /// </list>
    /// 
    /// The GroupNameFormat "'v'V" creates version groups like "v1", "v2" for Swagger documentation.
    /// </remarks>
    public static IServiceCollection AddTreasaraApiVersioning(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            // Set default API version to 1.0
            options.DefaultApiVersion = new ApiVersion(1, 0);
            
            // Assume default version when client doesn't specify one
            options.AssumeDefaultVersionWhenUnspecified = true;
            
            // Add "api-supported-versions" header to responses
            options.ReportApiVersions = true;
            
            // Read version from URL segment (e.g., /api/v1/...)
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        })
        .AddMvc()
        .AddApiExplorer(options =>
        {
            // Format version groups as "v1", "v2", etc. for Swagger
            options.GroupNameFormat = "'v'V";
            
            // Replace {version:apiVersion} placeholder in routes with actual version
            options.SubstituteApiVersionInUrl = true;
        });

        return services;
    }

    /// <summary>
    /// Configures rate limiting policies to protect the API from excessive request rates.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configuration">The application configuration containing rate limiting settings.</param>
    /// <returns>The service collection for method chaining.</returns>
    /// <remarks>
    /// Rate limiting helps protect the API from:
    /// <list type="bullet">
    /// <item><description>Denial-of-service (DoS) attacks</description></item>
    /// <item><description>Brute force authentication attempts</description></item>
    /// <item><description>Resource exhaustion from excessive requests</description></item>
    /// <item><description>Unintentional client errors causing request loops</description></item>
    /// </list>
    /// 
    /// Configuration is loaded from appsettings.json:
    /// <code>
    /// "RateLimiting": {
    ///   "PermitLimit": 30,      // Requests allowed per window
    ///   "WindowSeconds": 60,     // Time window in seconds
    ///   "QueueLimit": 0          // Max queued requests (0 = reject immediately)
    /// }
    /// </code>
    /// 
    /// The fixed window limiter:
    /// <list type="bullet">
    /// <item><description>Allows N requests per time window</description></item>
    /// <item><description>Returns HTTP 429 (Too Many Requests) when limit exceeded</description></item>
    /// <item><description>Processes queued requests oldest-first (FIFO)</description></item>
    /// <item><description>Resets at the end of each window</description></item>
    /// </list>
    /// 
    /// Default values (if configuration missing):
    /// <list type="bullet">
    /// <item><description>PermitLimit: 30 requests</description></item>
    /// <item><description>Window: 60 seconds</description></item>
    /// <item><description>QueueLimit: 0 (no queuing)</description></item>
    /// </list>
    /// </remarks>
    public static IServiceCollection AddTreasaraRateLimiting(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Bind rate limiting configuration to strongly-typed options
        services.Configure<RateLimitingOptions>(
            configuration.GetSection("RateLimiting"));

        // Load configuration values for immediate use
        var config = configuration
            .GetSection("RateLimiting")
            .Get<RateLimitingOptions>();

        services.AddRateLimiter(options =>
        {
            // Return HTTP 429 when rate limit exceeded
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // Configure fixed window rate limiter for public API
            options.AddFixedWindowLimiter(PublicApiRateLimitPolicy, limiterOptions =>
            {
                // Maximum requests allowed per window (default: 30)
                limiterOptions.PermitLimit = config?.PermitLimit ?? 30;
                
                // Time window duration (default: 60 seconds)
                limiterOptions.Window = TimeSpan.FromSeconds(config?.WindowSeconds ?? 60);
                
                // Maximum requests to queue when limit exceeded (default: 0, reject immediately)
                limiterOptions.QueueLimit = config?.QueueLimit ?? 0;
                
                // Process queued requests in FIFO order
                limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            });
        });

        return services;
    }

    /// <summary>
    /// Configures OpenAPI (Swagger) documentation generation and AutoMapper for DTO mappings.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The service collection for method chaining.</returns>
    /// <remarks>
    /// This method registers services for:
    /// <list type="bullet">
    /// <item><description><b>API Explorer</b>: Generates API metadata for Swagger</description></item>
    /// <item><description><b>Swagger Generator</b>: Creates OpenAPI specification documents</description></item>
    /// <item><description><b>AutoMapper</b>: Maps between domain objects and DTOs</description></item>
    /// </list>
    /// 
    /// AutoMapper scans the assembly containing <see cref="Program"/> for:
    /// <list type="bullet">
    /// <item><description>Classes inheriting from <c>Profile</c></description></item>
    /// <item><description>Mapping configurations between domain and DTO types</description></item>
    /// </list>
    /// 
    /// Examples of mapped types:
    /// <list type="bullet">
    /// <item><description>DateOnly ↔ LocalDate (NodaTime)</description></item>
    /// <item><description>ValuationResult&lt;Bond&gt; ↔ BondValuationResponseDto</description></item>
    /// <item><description>ValuationLine ↔ ValuationLineDto</description></item>
    /// </list>
    /// 
    /// Swagger UI is available at:
    /// <list type="bullet">
    /// <item><description><c>/swagger</c> - Interactive API documentation</description></item>
    /// <item><description><c>/swagger/v1/swagger.json</c> - OpenAPI specification</description></item>
    /// </list>
    /// </remarks>
    public static IServiceCollection AddTreasaraOpenApi(this IServiceCollection services)
    {
        // Register API Explorer for metadata generation
        services.AddEndpointsApiExplorer();
        
        // Register Swagger documentation generator
        services.AddSwaggerGen();
        
        // Register AutoMapper and scan for Profile classes in the API assembly
        services.AddAutoMapper(cfg => { }, typeof(Program));

        return services;
    }

    /// <summary>
    /// Registers application-specific services including exception handlers, mappers, and factories.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The service collection for method chaining.</returns>
    /// <remarks>
    /// This method registers core application services:
    /// 
    /// <b>Exception Handling:</b>
    /// <list type="bullet">
    /// <item><description><see cref="GlobalExceptionHandler"/> - Catches unhandled exceptions and returns standardized error responses</description></item>
    /// <item><description>ProblemDetails - RFC 7807 problem details support for error responses</description></item>
    /// </list>
    /// 
    /// <b>Request Processing:</b>
    /// <list type="bullet">
    /// <item><description><see cref="IBondRequestMapper"/> - Maps bond valuation request DTOs to domain objects</description></item>
    /// <item><description><see cref="IValidationErrorResponseFactory"/> - Creates standardized validation error responses</description></item>
    /// </list>
    /// 
    /// Service lifetimes:
    /// <list type="bullet">
    /// <item><description><b>Scoped</b>: Services are created once per HTTP request</description></item>
    /// <item><description>Appropriate for services that maintain request-specific state</description></item>
    /// <item><description>Disposed automatically at the end of the request</description></item>
    /// </list>
    /// </remarks>
    public static IServiceCollection AddTreasaraApplicationServices(this IServiceCollection services)
    {
        // Register RFC 7807 problem details for standardized error responses
        services.AddProblemDetails();
        
        // Register global exception handler for unhandled exceptions
        services.AddExceptionHandler<GlobalExceptionHandler>();

        // Register request mappers (scoped to HTTP request lifetime)
        services.AddScoped<IBondRequestMapper, BondRequestMapper>();
        
        // Register validation error response factory
        services.AddScoped<IValidationErrorResponseFactory, ValidationErrorResponseFactory>();

        return services;
    }

    /// <summary>
    /// Configures health check services for liveness and readiness probes.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The service collection for method chaining.</returns>
    /// <remarks>
    /// Health checks are essential for container orchestration platforms like Kubernetes
    /// to determine application health and readiness to serve traffic.
    /// 
    /// Default registration includes:
    /// <list type="bullet">
    /// <item><description>Basic health check service infrastructure</description></item>
    /// <item><description>No specific health checks registered (all checks pass by default)</description></item>
    /// </list>
    /// 
    /// Health check endpoints are mapped in <see cref="WebApplicationExtensions.MapTreasaraEndpoints"/>:
    /// <list type="bullet">
    /// <item><description><c>/health/live</c> - Liveness probe (is the app running?)</description></item>
    /// <item><description><c>/health/ready</c> - Readiness probe (is the app ready for traffic?)</description></item>
    /// </list>
    /// 
    /// Future enhancements could include:
    /// <list type="bullet">
    /// <item><description>Database connectivity checks</description></item>
    /// <item><description>External service availability checks</description></item>
    /// <item><description>Disk space and memory checks</description></item>
    /// <item><description>Custom business logic health indicators</description></item>
    /// </list>
    /// </remarks>
    public static IServiceCollection AddTreasaraHealthChecks(this IServiceCollection services)
    {
        // Register health check services (basic infrastructure)
        services.AddHealthChecks();
        
        // Note: Specific health checks can be added here:
        // services.AddHealthChecks()
        //     .AddDbContextCheck<AppDbContext>("database")
        //     .AddUrlGroup(new Uri("https://external-api.com"), "external-api");
        
        return services;
    }

    /// <summary>
    /// Configures Cross-Origin Resource Sharing (CORS) policies for frontend applications.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configuration">The application configuration containing CORS settings.</param>
    /// <returns>The service collection for method chaining.</returns>
    /// <remarks>
    /// CORS configuration allows web browsers to make requests from frontend applications
    /// running on different domains (origins) than the API.
    /// 
    /// Configuration is loaded from appsettings.json:
    /// <code>
    /// "CORS": {
    ///   "AllowedOrigins": [
    ///     "http://localhost:3000",     // React dev server
    ///     "https://app.example.com"    // Production frontend
    ///   ]
    /// }
    /// </code>
    /// 
    /// The CORS policy:
    /// <list type="bullet">
    /// <item><description><b>Allowed Origins</b>: Only origins specified in configuration can access the API</description></item>
    /// <item><description><b>Any Header</b>: Allows all request headers (Content-Type, Authorization, etc.)</description></item>
    /// <item><description><b>Any Method</b>: Allows all HTTP methods (GET, POST, PUT, DELETE, etc.)</description></item>
    /// <item><description><b>Exposed Headers</b>: Makes X-Correlation-Id header accessible to JavaScript</description></item>
    /// </list>
    /// 
    /// Security considerations:
    /// <list type="bullet">
    /// <item><description>Never use "*" for allowed origins in production</description></item>
    /// <item><description>Specify exact origins including protocol and port</description></item>
    /// <item><description>Keep the list of allowed origins as narrow as possible</description></item>
    /// <item><description>Review and update allowed origins when deploying new frontend apps</description></item>
    /// </list>
    /// 
    /// If no origins are configured, an empty array is used (effectively blocking CORS requests).
    /// </remarks>
    public static IServiceCollection AddTreasaraCors(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Load allowed origins from configuration (or empty array if not configured)
        var allowedOrigins = configuration
            .GetSection("CORS:AllowedOrigins")
            .Get<string[]>() ?? [];

        services.AddCors(options =>
        {
            options.AddPolicy(CorsPolicyName, policy =>
            {
                // Restrict access to specific origins (never use AllowAnyOrigin in production)
                policy.WithOrigins(allowedOrigins)
                      .AllowAnyHeader()              // Allow all request headers
                      .AllowAnyMethod()              // Allow all HTTP methods
                      .WithExposedHeaders("X-Correlation-Id");  // Expose correlation ID to JavaScript
            });
        });

        return services;
    }

    /// <summary>
    /// Registers FluentValidation validators for request DTO validation.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The service collection for method chaining.</returns>
    /// <remarks>
    /// FluentValidation provides a fluent API for defining validation rules on request DTOs.
    /// Validators are registered with scoped lifetime, meaning a new instance is created
    /// per HTTP request.
    /// 
    /// Currently registered validators:
    /// <list type="bullet">
    /// <item><description><see cref="BondValuationRequestValidator"/> - Validates bond valuation request parameters</description></item>
    /// </list>
    /// 
    /// Validation rules enforced by <see cref="BondValuationRequestValidator"/>:
    /// <list type="bullet">
    /// <item><description>Notional must be positive</description></item>
    /// <item><description>Coupon rate must be non-negative</description></item>
    /// <item><description>Required fields must not be empty (currency, frequency, day count, roll convention)</description></item>
    /// <item><description>Maturity date must be after issue date</description></item>
    /// <item><description>Valuation date must not be before issue date</description></item>
    /// </list>
    /// 
    /// Validators are invoked in controllers before processing requests, providing
    /// early validation and clear error messages to API consumers.
    /// 
    /// To add new validators:
    /// <code>
    /// services.AddScoped&lt;IValidator&lt;SwapValuationRequestDto&gt;, SwapValuationRequestValidator&gt;();
    /// </code>
    /// </remarks>
    public static IServiceCollection AddTreasaraValidators(this IServiceCollection services)
    {
        // Register FluentValidation validators (scoped lifetime)
        services.AddScoped<IValidator<BondValuationRequestDto>, BondValuationRequestValidator>();
        
        // Future validators can be added here:
        // services.AddScoped<IValidator<SwapValuationRequestDto>, SwapValuationRequestValidator>();
        
        return services;
    }
}