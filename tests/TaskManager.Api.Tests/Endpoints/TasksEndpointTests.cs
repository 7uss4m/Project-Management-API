using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using TaskManager.Application.DTOs.Common;
using TaskManager.Application.DTOs.Tasks;
using TaskManager.Domain.Enums;
using TaskManager.Api.Tests.Infrastructure;

namespace TaskManager.Api.Tests.Endpoints;

public class TasksEndpointTests : TestBase
{
    public TasksEndpointTests(ApiFactory factory) : base(factory) { }

    [Fact]
    public async Task POST_Tasks_WithValidBody_Returns201WithLocation()
    {
        var project = await CreateProjectAsync();

        var response = await Client.PostAsJsonAsync($"/projects/{project.Id}/tasks", new
        {
            title = "Fix bug",
            priority = "High"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
    }

    [Fact]
    public async Task GET_Tasks_ReturnsPaginationMetadata()
    {
        var project = await CreateProjectAsync();
        await CreateTaskAsync(project.Id, "T1");
        await CreateTaskAsync(project.Id, "T2");

        var response = await Client.GetAsync($"/projects/{project.Id}/tasks?page=1&pageSize=10");
        var result = await ReadAsync<PagedResult<TaskDto>>(response);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.TotalCount.Should().Be(2);
        result.Page.Should().Be(1);
    }

    [Fact]
    public async Task GET_Tasks_FilteredByStatus_ReturnsOnlyMatchingTasks()
    {
        var project = await CreateProjectAsync();
        var task1 = await CreateTaskAsync(project.Id, "Todo Task");
        await Client.PatchAsJsonAsync($"/projects/{project.Id}/tasks/{task1.Id}/status", new { status = "InProgress" });
        await CreateTaskAsync(project.Id, "Another Todo");

        var response = await Client.GetAsync($"/projects/{project.Id}/tasks?status=InProgress");
        var result = await ReadAsync<PagedResult<TaskDto>>(response);

        result.Items.Should().OnlyContain(t => t.Status == ProjectTaskStatus.InProgress);
    }

    [Fact]
    public async Task GET_Tasks_WithInvalidStatus_Returns400()
    {
        var project = await CreateProjectAsync();

        var response = await Client.GetAsync($"/projects/{project.Id}/tasks?status=InvalidValue");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GET_Tasks_WithInvalidPriority_Returns400()
    {
        var project = await CreateProjectAsync();

        var response = await Client.GetAsync($"/projects/{project.Id}/tasks?priority=NotAPriority");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GET_Tasks_WithNonExistentAssigneeId_Returns400()
    {
        var project = await CreateProjectAsync();

        var response = await Client.GetAsync($"/projects/{project.Id}/tasks?assigneeId=99999");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DELETE_Task_WhenNotFound_Returns404()
    {
        var project = await CreateProjectAsync();

        var response = await Client.DeleteAsync($"/projects/{project.Id}/tasks/99999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PATCH_Task_Status_UpdatesAndReturnsTask()
    {
        var project = await CreateProjectAsync();
        var task = await CreateTaskAsync(project.Id);

        var response = await Client.PatchAsJsonAsync(
            $"/projects/{project.Id}/tasks/{task.Id}/status",
            new { status = "Done" });
        var updated = await ReadAsync<TaskDto>(response);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        updated.Status.Should().Be(ProjectTaskStatus.Done);
    }

    [Fact]
    public async Task PATCH_Task_Assignee_SetsAssignee()
    {
        var project = await CreateProjectAsync();
        var user = await CreateUserAsync();
        var task = await CreateTaskAsync(project.Id);

        var response = await Client.PatchAsJsonAsync(
            $"/projects/{project.Id}/tasks/{task.Id}/assignee",
            new { assigneeId = user.Id });
        var updated = await ReadAsync<TaskDto>(response);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        updated.AssigneeId.Should().Be(user.Id);
    }

    [Fact]
    public async Task GET_Tasks_FiltersByMultipleParams()
    {
        var project = await CreateProjectAsync();
        var user = await CreateUserAsync();
        var t1 = await CreateTaskAsync(project.Id, "High Prio", Priority.High);
        await Client.PatchAsJsonAsync($"/projects/{project.Id}/tasks/{t1.Id}/assignee", new { assigneeId = user.Id });
        await Client.PatchAsJsonAsync($"/projects/{project.Id}/tasks/{t1.Id}/status", new { status = "InProgress" });
        await CreateTaskAsync(project.Id, "Low Prio", Priority.Low);

        var response = await Client.GetAsync(
            $"/projects/{project.Id}/tasks?status=InProgress&priority=High&assigneeId={user.Id}");
        var result = await ReadAsync<PagedResult<TaskDto>>(response);

        result.Items.Should().HaveCount(1);
        result.Items[0].Title.Should().Be("High Prio");
    }
}
