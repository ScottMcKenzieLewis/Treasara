using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Treasara.Api.Configuration;
using Treasara.Api.Health;
using Treasara.Api.Middleware;

namespace Treasara.Api.Extensions;

/// <summary>
/// Extension methods for configuring the application's middleware pipeline and endpoint routing.
/// </summary>
/// <remarks>
/// This class provides extension methods for setting up the HTTP request processing pipeline
/// and mapping API endpoints. The methods are organized into two main categories:
/// 
/// <b>Pipeline Configuration:</b>
/// <list type="bullet">
/// <item><description>Exception handling and error responses</description></item>
/// <item><description>Development tools (Swagger, OpenAPI)</description></item>
/// <item><description>Security features (HTTPS, CORS, header cleanup)</description></item>
/// <item><description>Rate limiting and throttling</description></item>
/// <item><description>Request correlation and logging</description></item>
/// </list>
/// 
/// <b>Endpoint Mapping:</b>
/// <list type="bullet">
/// <item><description>Health check endpoints (liveness and readiness probes)</description></item>
/// <item><description>API controllers with rate limiting</description></item>
/// </list>
/// 
/// Middleware ordering is critical:
/// The order in which middleware is registered determines the order in which it processes
/// requests (top-to-bottom) and responses (bottom-to-top). Incorrect ordering can cause:
/// <list type="bullet">
/// <item><description>Security vulnerabilities (e.g., CORS after authentication)</description></item>
/// <item><description>Missing correlation IDs in logs</description></item>
/// <item><description>Incomplete request timing data</description></item>
/// <item><description>Rate limiting not applied correctly</description></item>
/// </list>
/// 
/// The extension method pattern:
/// <list type="bullet">
/// <item><description>Keeps Program.cs clean and focused on application structure</description></item>
/// <item><description>Groups related configuration into logical units</description></item>
/// <item><description>Makes middleware ordering explicit and documented</description></item>
/// <item><description>Enables reusability across different hosting scenarios</description></item>
/// </list>
/// </remarks>
public static class WebApplicationExtensions
{
    /// <summary>
    /// Configures the complete HTTP request processing pipeline for the Treasara API.
    /// </summary>
    /// <param name="app">The web application to configure.</param>
    /// <returns>The web application for method chaining.</returns>
    /// <remarks>
    /// This method establishes the middleware pipeline in a carefully ordered sequence.
    /// Each middleware component is registered in the order it should process requests.
    /// 
    /// Middleware execution order (request flow):
    /// <list type="number">
    /// <item><description><b>Exception Handler</b> - Catches unhandled exceptions from all subsequent middleware</description></item>
    /// <item><description><b>Development Tools</b> - Swagger UI and OpenAPI (Development environment only)</description></item>
    /// <item><description><b>HTTPS Redirection</b> - Redirects HTTP requests to HTTPS</description></item>
    /// <item><description><b>CORS</b> - Evaluates cross-origin policies before authentication</description></item>
    /// <item><description><b>Security Headers</b> - Removes information disclosure headers</description></item>
    /// <item><description><b>Rate Limiter</b> - Applies throttling policies to prevent abuse</description></item>
    /// <item><description><b>Correlation ID</b> - Establishes request correlation for tracing</description></item>
    /// <item><description><b>Request Logging</b> - Logs request details with timing information</description></item>
    /// <item><description><b>Controllers/Endpoints</b> - Actual request processing (mapped separately)</description></item>
    /// </list>
    /// 
    /// Response flow is reversed (bottom-to-top), allowing each middleware to:
    /// <list type="bullet">
    /// <item><description>Add headers to responses (correlation ID, security headers)</description></item>
    /// <item><description>Log completion information (request logging)</description></item>
    /// <item><description>Transform responses (exception handler)</description></item>
    /// </list>
    /// 
    /// Key ordering decisions:
    /// <list type="bullet">
    /// <item><description><b>Exception handler first</b>: Catches all errors including from other middleware</description></item>
    /// <item><description><b>CORS before authentication</b>: Allows preflight requests without auth</description></item>
    /// <item><description><b>Correlation ID before logging</b>: Ensures logs include correlation IDs</description></item>
    /// <item><description><b>Rate limiting before endpoints</b>: Reject excess requests early</description></item>
    /// </list>
    /// 
    /// Environment-specific behavior:
    /// <list type="bullet">
    /// <item><description><b>Development</b>: Enables Swagger UI and OpenAPI explorer</description></item>
    /// <item><description><b>Production</b>: Disables development tools for security and performance</description></item>
    /// </list>
    /// 
    /// This method is called from Program.cs:
    /// <code>
    /// app.UseTreasaraApiPipeline();
    /// </code>
    /// </remarks>
    public static WebApplication UseTreasaraApiPipeline(this WebApplication app)
    {
        // Exception handler - Must be first to catch all errors
        // SuppressDiagnosticsCallback = false ensures exceptions are logged
        app.UseExceptionHandler(new ExceptionHandlerOptions
        {
            SuppressDiagnosticsCallback = _ => false
        });

        // Development-only tools: Swagger UI and OpenAPI
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();      // Map OpenAPI specification endpoint
            app.UseSwagger();      // Serve OpenAPI JSON
            app.UseSwaggerUI();    // Serve interactive Swagger UI
        }

        // HTTPS redirection - Redirect HTTP to HTTPS
        app.UseHttpsRedirection();

        // CORS - Must be before authentication to allow preflight requests
        app.UseCors(ServiceCollectionExtensions.CorsPolicyName);

        // Security header cleanup - Remove information disclosure headers
        app.UseSecurityHeaderCleanup();

        // Rate limiting - Throttle excessive requests
        app.UseRateLimiter();

        // Correlation ID - Establish distributed tracing context
        // Must be before logging so correlation IDs appear in logs
        app.UseCorrelationId();

        // Request logging - Log all requests with timing and correlation data
        app.UseRequestLogging();

        // Note: Controllers are mapped in MapTreasaraEndpoints()

        return app;
    }

    /// <summary>
    /// Maps all API endpoints including health checks and controllers.
    /// </summary>
    /// <param name="app">The web application to configure.</param>
    /// <returns>The web application for method chaining.</returns>
    /// <remarks>
    /// This method configures endpoint routing for the application, including:
    /// 
    /// <b>Health Check Endpoints:</b>
    /// <list type="bullet">
    /// <item><description><c>/health/live</c> - Liveness probe for container orchestration</description></item>
    /// <item><description><c>/health/ready</c> - Readiness probe for load balancer routing</description></item>
    /// </list>
    /// 
    /// Both health check endpoints use a custom response writer (<see cref="HealthCheckResponseWriter.WriteResponse"/>)
    /// that provides detailed JSON output including:
    /// <list type="bullet">
    /// <item><description>Overall health status (Healthy, Degraded, Unhealthy)</description></item>
    /// <item><description>Individual check results with timing</description></item>
    /// <item><description>Total duration of health check execution</description></item>
    /// <item><description>Trace ID for correlation with logs</description></item>
    /// </list>
    /// 
    /// <b>Controller Endpoints:</b>
    /// All MVC controllers are mapped with automatic rate limiting applied via the
    /// "public-api" policy. This ensures consistent throttling across all API endpoints.
    /// 
    /// Health checks vs. Controllers:
    /// <list type="bullet">
    /// <item><description><b>Health checks</b>: No rate limiting (must always be accessible for monitoring)</description></item>
    /// <item><description><b>Controllers</b>: Rate limited to protect against abuse</description></item>
    /// </list>
    /// 
    /// Kubernetes integration:
    /// <code>
    /// livenessProbe:
    ///   httpGet:
    ///     path: /health/live
    ///     port: 8080
    ///   initialDelaySeconds: 3
    ///   periodSeconds: 10
    /// 
    /// readinessProbe:
    ///   httpGet:
    ///     path: /health/ready
    ///     port: 8080
    ///   initialDelaySeconds: 5
    ///   periodSeconds: 5
    /// </code>
    /// 
    /// This method is called from Program.cs:
    /// <code>
    /// app.MapTreasaraEndpoints();
    /// </code>
    /// </remarks>
    public static WebApplication MapTreasaraEndpoints(this WebApplication app)
    {
        // Map liveness health check endpoint
        // Used by Kubernetes to determine if the container should be restarted
        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            ResponseWriter = HealthCheckResponseWriter.WriteResponse
        });

        // Map readiness health check endpoint
        // Used by load balancers to determine if the app can receive traffic
        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            ResponseWriter = HealthCheckResponseWriter.WriteResponse
        });

        // Map all controller endpoints with rate limiting
        // Rate limiting protects the API from abuse and resource exhaustion
        app.MapControllers()
           .RequireRateLimiting(ServiceCollectionExtensions.PublicApiRateLimitPolicy);

        return app;
    }

    /// <summary>
    /// Registers middleware to remove security-sensitive HTTP response headers.
    /// </summary>
    /// <param name="app">The application builder to configure.</param>
    /// <returns>The application builder for method chaining.</returns>
    /// <remarks>
    /// This middleware removes HTTP headers that may disclose information about the
    /// server implementation, framework versions, or technology stack. Header removal
    /// is part of a defense-in-depth security strategy.
    /// 
    /// Headers are configured in appsettings.json:
    /// <code>
    /// "SecurityHeaders": {
    ///   "Remove": [
    ///     "Server",           // Web server type and version
    ///     "X-Powered-By",     // Framework information
    ///     "X-AspNet-Version", // ASP.NET version
    ///     "X-AspNetMvc-Version" // ASP.NET MVC version
    ///   ]
    /// }
    /// </code>
    /// 
    /// Why remove these headers:
    /// <list type="bullet">
    /// <item><description><b>Reduces attack surface</b>: Attackers can't easily identify framework versions</description></item>
    /// <item><description><b>Prevents targeted exploits</b>: Known vulnerabilities are harder to find</description></item>
    /// <item><description><b>Compliance</b>: Some security standards require header removal</description></item>
    /// <item><description><b>Professional appearance</b>: Doesn't advertise technology choices</description></item>
    /// </list>
    /// 
    /// Implementation notes:
    /// <list type="bullet">
    /// <item><description>Headers are removed using Response.OnStarting callback</description></item>
    /// <item><description>Removal happens just before the response is sent</description></item>
    /// <item><description>Configuration is optional; no headers removed if not configured</description></item>
    /// <item><description>Minimal performance impact (header removal is fast)</description></item>
    /// </list>
    /// 
    /// This is a custom inline middleware rather than a separate class because:
    /// <list type="bullet">
    /// <item><description>Simple logic that doesn't warrant a full middleware class</description></item>
    /// <item><description>Configuration access is straightforward via IApplicationBuilder</description></item>
    /// <item><description>Keeps related configuration cleanup in one place</description></item>
    /// </list>
    /// </remarks>
    public static IApplicationBuilder UseSecurityHeaderCleanup(this IApplicationBuilder app)
    {
        // Retrieve configuration from DI container
        var configuration = app.ApplicationServices.GetRequiredService<IConfiguration>();
        
        // Load security headers configuration
        var options = configuration
            .GetSection("SecurityHeaders")
            .Get<SecurityHeadersOptions>();

        // Register middleware using inline delegate
        return app.Use(async (context, next) =>
        {
            // Register callback to remove headers just before response is sent
            context.Response.OnStarting(() =>
            {
                // Remove configured headers if any are specified
                if (options?.Remove is not null)
                {
                    foreach (var header in options.Remove)
                    {
                        context.Response.Headers.Remove(header);
                    }
                }

                return Task.CompletedTask;
            });

            // Continue to next middleware
            await next();
        });
    }

    /// <summary>
    /// Adds the <see cref="CorrelationIdMiddleware"/> to the application's request pipeline.
    /// </summary>
    /// <param name="app">The application builder to configure.</param>
    /// <returns>The application builder for method chaining.</returns>
    /// <remarks>
    /// This extension method registers the correlation ID middleware, which establishes
    /// distributed tracing context for each request. It should be registered early in
    /// the pipeline so that correlation IDs are available to all subsequent middleware
    /// and logging operations.
    /// 
    /// The middleware:
    /// <list type="bullet">
    /// <item><description>Extracts correlation IDs from X-Correlation-ID request headers</description></item>
    /// <item><description>Generates new ULIDs if no correlation ID is provided</description></item>
    /// <item><description>Adds correlation IDs to response headers for client tracking</description></item>
    /// <item><description>Creates log scopes with correlation IDs for automatic log enrichment</description></item>
    /// </list>
    /// 
    /// See <see cref="CorrelationIdMiddleware"/> for detailed implementation information.
    /// 
    /// Note: This method duplicates functionality from <see cref="CorrelationIdExtensions.UseCorrelationId"/>
    /// Both are kept for flexibility in different usage scenarios.
    /// </remarks>
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
    {
        return app.UseMiddleware<CorrelationIdMiddleware>();
    }

    /// <summary>
    /// Adds the <see cref="RequestLoggingMiddleware"/> to the application's request pipeline.
    /// </summary>
    /// <param name="app">The application builder to configure.</param>
    /// <returns>The application builder for method chaining.</returns>
    /// <remarks>
    /// This extension method registers the request logging middleware, which logs all
    /// HTTP requests with timing information. It should be registered after correlation
    /// ID middleware to ensure logs include correlation IDs.
    /// 
    /// The middleware logs:
    /// <list type="bullet">
    /// <item><description>Incoming request method and path</description></item>
    /// <item><description>Request completion status code</description></item>
    /// <item><description>Request processing duration in milliseconds</description></item>
    /// <item><description>Correlation ID and trace ID via log scopes</description></item>
    /// </list>
    /// 
    /// See <see cref="RequestLoggingMiddleware"/> for detailed implementation information.
    /// 
    /// Note: This method duplicates functionality from <see cref="RequestLoggingMiddlewareExtensions.UseRequestLogging"/>
    /// Both are kept for flexibility in different usage scenarios.
    /// </remarks>
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
    {
        return app.UseMiddleware<RequestLoggingMiddleware>();
    }
}