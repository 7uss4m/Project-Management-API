using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TaskManager.Application.DTOs.Common;
using TaskManager.Application.DTOs.Users;
using TaskManager.Api.Tests.Infrastructure;

namespace TaskManager.Api.Tests.Endpoints;

public class UsersEndpointTests : TestBase
{
    public UsersEndpointTests(ApiFactory factory) : base(factory) { }

    [Fact]
    public async Task POST_Users_WithValidBody_Returns201WithLocation()
    {
        var response = await Client.PostAsJsonAsync("/users",
            new { name = "Alice", email = $"alice-{Guid.NewGuid()}@test.com", password = "AlicePw123!" });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
    }

    [Fact]
    public async Task GET_Users_ReturnsPaginationMetadata()
    {
        await CreateUserAsync("User1");
        await CreateUserAsync("User2");

        var response = await Client.GetAsync("/users?page=1&pageSize=10");
        var result = await ReadAsync<PagedResult<UserDto>>(response);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.TotalCount.Should().BeGreaterThanOrEqualTo(2);
        result.TotalPages.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task GET_Users_ById_WhenNotFound_Returns404()
    {
        var response = await Client.GetAsync("/users/99999");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DELETE_User_Returns204AndSoftDeletes()
    {
        var user = await CreateUserAsync("DeleteMe");

        var deleteResponse = await Client.DeleteAsync($"/users/{user.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await Client.GetAsync($"/users/{user.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
