using System.Net;
using System.Text.Json;
using FluentAssertions;
using Treasara.Api.Tests.Infrastructure;
using Xunit;

namespace Treasara.Api.Tests.Integration;

/// <summary>
/// Integration tests for health check endpoints.
/// </summary>
/// <remarks>
/// These tests verify that the application's health check endpoints are properly configured
/// and responding correctly. Health checks are critical for container orchestration platforms
/// (like Kubernetes) and load balancers to determine application health and readiness.
/// 
/// The two types of health checks serve different purposes:
/// <list type="bullet">
/// <item><description><b>Liveness probe (/health/live)</b>: Indicates if the application is running and not deadlocked</description></item>
/// <item><description><b>Readiness probe (/health/ready)</b>: Indicates if the application is ready to accept traffic</description></item>
/// </list>
/// 
/// These tests ensure that:
/// <list type="bullet">
/// <item><description>Health check endpoints are accessible</description></item>
/// <item><description>Endpoints return HTTP 200 OK when healthy</description></item>
/// <item><description>Health check middleware is properly configured</description></item>
/// </list>
/// 
/// In production, orchestration platforms use these endpoints to:
/// <list type="bullet">
/// <item><description>Restart unhealthy containers (liveness failures)</description></item>
/// <item><description>Remove containers from load balancer rotation (readiness failures)</description></item>
/// <item><description>Determine when to route traffic to new deployments</description></item>
/// </list>
/// </remarks>
public sealed class HealthChecksIntegrationTests : IClassFixture<TreasaraApiFactory>
{
    private readonly HttpClient _client;

    private const string LivenessEndpoint = "/health/live";
    private const string ReadinessEndpoint = "/health/ready";

    /// <summary>
    /// Initializes a new instance of the <see cref="HealthChecksIntegrationTests"/> class.
    /// </summary>
    /// <param name="factory">The web application factory for creating test HTTP clients.</param>
    public HealthChecksIntegrationTests(TreasaraApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    /// <summary>
    /// Verifies that the liveness health check endpoint returns HTTP 200 OK.
    /// </summary>
    /// <remarks>
    /// The liveness probe indicates whether the application is running and not in a deadlock state.
    /// A failing liveness check typically indicates that the application should be restarted.
    /// 
    /// This endpoint should:
    /// <list type="bullet">
    /// <item><description>Return quickly (within a few hundred milliseconds)</description></item>
    /// <item><description>Not depend on external services (databases, APIs, etc.)</description></item>
    /// <item><description>Check only internal application state</description></item>
    /// </list>
    /// 
    /// In Kubernetes, a failing liveness probe triggers a pod restart. This test ensures
    /// the endpoint is properly configured and accessible.
    /// </remarks>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Live_Should_Return200Ok()
    {
        // Act
        var response = await _client.GetAsync(LivenessEndpoint);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>
    /// Verifies that the readiness health check endpoint returns HTTP 200 OK.
    /// </summary>
    /// <remarks>
    /// The readiness probe indicates whether the application is ready to serve traffic.
    /// A failing readiness check means the application is alive but temporarily unable
    /// to handle requests (e.g., still initializing, database connection issues, etc.).
    /// 
    /// This endpoint typically checks:
    /// <list type="bullet">
    /// <item><description>Database connectivity</description></item>
    /// <item><description>Required external service availability</description></item>
    /// <item><description>Application initialization completion</description></item>
    /// <item><description>Resource availability (memory, disk space)</description></item>
    /// </list>
    /// 
    /// In Kubernetes, a failing readiness probe removes the pod from service endpoints,
    /// preventing traffic from reaching it until it recovers. This test ensures the
    /// endpoint is properly configured and returns success when dependencies are healthy.
    /// </remarks>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Ready_Should_Return200Ok()
    {
        // Act
        var response = await _client.GetAsync(ReadinessEndpoint);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>
    /// Verifies that the liveness health check returns a valid JSON response structure.
    /// </summary>
    /// <remarks>
    /// This test validates that the custom health check response format is working correctly
    /// and includes the expected JSON structure with status, checks, and trace information.
    /// 
    /// The response should include:
    /// <list type="bullet">
    /// <item><description>Overall status (Healthy, Degraded, or Unhealthy)</description></item>
    /// <item><description>Total duration of all health checks</description></item>
    /// <item><description>Individual check results</description></item>
    /// <item><description>Trace ID for correlation</description></item>
    /// </list>
    /// </remarks>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Live_Should_Return_Valid_Json_Structure()
    {
        // Act
        var response = await _client.GetAsync(LivenessEndpoint);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");

        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);

        // Verify the JSON structure contains expected properties
        json.RootElement.TryGetProperty("status", out _).Should().BeTrue();
        json.RootElement.TryGetProperty("totalDuration", out _).Should().BeTrue();
        json.RootElement.TryGetProperty("checks", out _).Should().BeTrue();
        json.RootElement.TryGetProperty("traceId", out _).Should().BeTrue();

        // Verify status is a valid health status
        var status = json.RootElement.GetProperty("status").GetString();
        status.Should().BeOneOf("Healthy", "Degraded", "Unhealthy");
    }

    /// <summary>
    /// Verifies that the readiness health check returns a valid JSON response structure.
    /// </summary>
    /// <remarks>
    /// Similar to the liveness check, this validates the readiness endpoint returns
    /// properly formatted JSON with all required health check information.
    /// </remarks>
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Ready_Should_Return_Valid_Json_Structure()
    {
        // Act
        var response = await _client.GetAsync(ReadinessEndpoint);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");

        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);

        // Verify the JSON structure contains expected properties
        json.RootElement.TryGetProperty("status", out _).Should().BeTrue();
        json.RootElement.TryGetProperty("totalDuration", out _).Should().BeTrue();
        json.RootElement.TryGetProperty("checks", out _).Should().BeTrue();
        json.RootElement.TryGetProperty("traceId", out _).Should().BeTrue();

        // Verify status is a valid health status
        var status = json.RootElement.GetProperty("status").GetString();
        status.Should().BeOneOf("Healthy", "Degraded", "Unhealthy");
    }
}