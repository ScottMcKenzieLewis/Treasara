using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;

namespace Treasara.Api.Tests.Infrastructure;

/// <summary>
/// Custom web application factory for creating in-memory test instances of the Treasara API.
/// </summary>
/// <remarks>
/// This factory extends <see cref="WebApplicationFactory{TEntryPoint}"/> to provide integration
/// testing capabilities for the Treasara API. It creates an in-memory test server that can be
/// used to make actual HTTP requests without requiring a deployed environment or external dependencies.
/// 
/// Key benefits of using this factory:
/// <list type="bullet">
/// <item><description>Fast integration tests that run entirely in-memory</description></item>
/// <item><description>No need for deployed environments or Docker containers</description></item>
/// <item><description>Automatic cleanup after each test run</description></item>
/// <item><description>Consistent test environment configuration</description></item>
/// <item><description>Easy dependency injection container access for test setup</description></item>
/// </list>
/// 
/// The factory is used with xUnit's <c>IClassFixture&lt;TreasaraApiFactory&gt;</c> to share
/// a single test server instance across all tests in a test class, improving test performance.
/// 
/// Environment configuration:
/// The factory overrides the application environment to "Testing", which can be used to:
/// <list type="bullet">
/// <item><description>Load test-specific configuration from appsettings.Testing.json</description></item>
/// <item><description>Enable or disable features specifically for testing</description></item>
/// <item><description>Configure different logging levels for test runs</description></item>
/// <item><description>Use in-memory databases or mock external services</description></item>
/// </list>
/// </remarks>
/// <example>
/// Example usage in a test class:
/// <code>
/// public sealed class MyIntegrationTests : IClassFixture&lt;TreasaraApiFactory&gt;
/// {
///     private readonly HttpClient _client;
///     
///     public MyIntegrationTests(TreasaraApiFactory factory)
///     {
///         _client = factory.CreateClient();
///     }
///     
///     [Fact]
///     public async Task Test_Endpoint()
///     {
///         var response = await _client.GetAsync("/api/v1/endpoint");
///         response.EnsureSuccessStatusCode();
///     }
/// }
/// </code>
/// </example>
public sealed class TreasaraApiFactory : WebApplicationFactory<Program>
{
    /// <summary>
    /// Creates and configures the host for the test server.
    /// </summary>
    /// <param name="builder">The host builder to configure.</param>
    /// <returns>The configured host instance.</returns>
    /// <remarks>
    /// This method overrides the default host creation to set the application environment
    /// to "Testing". This allows the application to:
    /// <list type="bullet">
    /// <item><description>Load test-specific configuration from appsettings.Testing.json</description></item>
    /// <item><description>Skip certain middleware or features during testing (e.g., authentication)</description></item>
    /// <item><description>Use test doubles or in-memory implementations for external dependencies</description></item>
    /// <item><description>Apply different logging or telemetry settings for test runs</description></item>
    /// </list>
    /// 
    /// The "Testing" environment name is distinct from "Development" and "Production",
    /// allowing complete isolation of test configuration from other environments.
    /// </remarks>
    protected override IHost CreateHost(IHostBuilder builder)
    {
        // Set the environment to "Testing" for test-specific configuration
        builder.UseEnvironment("Testing");

        return base.CreateHost(builder);
    }
}