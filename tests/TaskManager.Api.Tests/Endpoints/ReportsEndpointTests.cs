using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TaskManager.Application.Reports;
using TaskManager.Api.Tests.Infrastructure;

namespace TaskManager.Api.Tests.Endpoints;

public class ReportsEndpointTests : TestBase
{
    public ReportsEndpointTests(ApiFactory factory) : base(factory) { }

    [Fact]
    public async Task POST_Reports_TasksByStatus_Returns200WithCorrectShape()
    {
        var project = await CreateProjectAsync();
        await CreateTaskAsync(project.Id, "Task A");
        await CreateTaskAsync(project.Id, "Task B");

        var response = await Client.PostAsJsonAsync("/reports", new
        {
            type = "tasks_by_status",
            parameters = new { projectId = project.Id }
        });
        var result = await ReadAsync<ReportResult>(response);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.ReportType.Should().Be("tasks_by_status");
        result.ProjectId.Should().Be(project.Id);
        result.Labels.Should().NotBeEmpty();
        result.Series.Should().HaveCount(1);
        result.Series[0].Data.Sum().Should().Be(2);
    }

    [Fact]
    public async Task POST_Reports_UnknownType_Returns400()
    {
        var response = await Client.PostAsJsonAsync("/reports", new
        {
            type = "unknown_report_type",
            parameters = new { }
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
