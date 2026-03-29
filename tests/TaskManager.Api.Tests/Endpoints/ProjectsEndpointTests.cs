using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TaskManager.Application.DTOs.Common;
using TaskManager.Application.DTOs.Projects;
using TaskManager.Api.Tests.Infrastructure;

namespace TaskManager.Api.Tests.Endpoints;

public class ProjectsEndpointTests : TestBase
{
    public ProjectsEndpointTests(ApiFactory factory) : base(factory) { }

    [Fact]
    public async Task POST_Projects_WithValidBody_Returns201WithLocation()
    {
        var response = await Client.PostAsJsonAsync("/projects", new { name = "My Project" });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
    }

    [Fact]
    public async Task GET_Projects_ReturnsPaginationMetadataWithTaskCount()
    {
        var project = await CreateProjectAsync();
        await CreateTaskAsync(project.Id, "Task A");
        await CreateTaskAsync(project.Id, "Task B");

        var response = await Client.GetAsync("/projects?page=1&pageSize=20");
        var result = await ReadAsync<PagedResult<ProjectDto>>(response);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Page.Should().Be(1);
        result.TotalCount.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task GET_Projects_ById_WhenNotFound_Returns404()
    {
        var response = await Client.GetAsync("/projects/99999");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DELETE_Project_Returns204AndCascadesSoftDeleteToTasks()
    {
        var project = await CreateProjectAsync();
        var task = await CreateTaskAsync(project.Id, "Cascade Task");

        var deleteResponse = await Client.DeleteAsync($"/projects/{project.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getProjectResponse = await Client.GetAsync($"/projects/{project.Id}");
        getProjectResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var getTaskResponse = await Client.GetAsync($"/projects/{project.Id}/tasks/{task.Id}");
        getTaskResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task POST_Members_WithValidUserId_Returns201()
    {
        var project = await CreateProjectAsync();
        var user = await CreateUserAsync();

        var response = await Client.PostAsJsonAsync($"/projects/{project.Id}/members", new { userId = user.Id });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task POST_Members_WhenDuplicateMember_Returns409()
    {
        var project = await CreateProjectAsync();
        var user = await CreateUserAsync();
        await Client.PostAsJsonAsync($"/projects/{project.Id}/members", new { userId = user.Id });

        var response = await Client.PostAsJsonAsync($"/projects/{project.Id}/members", new { userId = user.Id });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task DELETE_Member_Returns204()
    {
        var project = await CreateProjectAsync();
        var user = await CreateUserAsync();
        await Client.PostAsJsonAsync($"/projects/{project.Id}/members", new { userId = user.Id });

        var response = await Client.DeleteAsync($"/projects/{project.Id}/members/{user.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
