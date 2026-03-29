using FluentAssertions;
using TaskManager.Api.Tests.Infrastructure;

namespace TaskManager.Api.Tests.Endpoints;

public class HealthEndpointTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task HealthLive_returns_200_without_authentication()
    {
        var response = await _client.GetAsync("/health/live");
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    [Fact]
    public async Task HealthReady_returns_200_when_database_is_available()
    {
        var response = await _client.GetAsync("/health/ready");
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    [Fact]
    public async Task Health_returns_200_when_all_checks_pass()
    {
        var response = await _client.GetAsync("/health");
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }
}
