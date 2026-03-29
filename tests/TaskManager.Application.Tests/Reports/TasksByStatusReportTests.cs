using FluentAssertions;
using Moq;
using TaskManager.Application.Reports;
using TaskManager.Application.Reports.Generators;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.Tests.Reports;

public class TasksByStatusReportTests
{
    private readonly Mock<ITaskRepository> _taskRepo = new();
    private readonly TasksByStatusReport _sut;

    public TasksByStatusReportTests() => _sut = new TasksByStatusReport(_taskRepo.Object);

    [Fact]
    public async Task GenerateAsync_ReturnsAllStatusesAsLabels()
    {
        var tasks = new List<ProjectTask>
        {
            new() { Status = ProjectTaskStatus.Todo },
            new() { Status = ProjectTaskStatus.Todo },
            new() { Status = ProjectTaskStatus.InProgress },
            new() { Status = ProjectTaskStatus.Done }
        };
        _taskRepo.Setup(r => r.GetAllForProjectAsync(1, default)).ReturnsAsync(tasks);

        var result = await _sut.GenerateAsync(new ReportRequest
        {
            Type = "tasks_by_status",
            Parameters = new ReportParameters { ProjectId = 1 }
        });

        result.Labels.Should().Contain(nameof(ProjectTaskStatus.Todo));
        result.Labels.Should().Contain(nameof(ProjectTaskStatus.InProgress));
        result.Labels.Should().Contain(nameof(ProjectTaskStatus.Done));
        result.Labels.Should().HaveCount(Enum.GetValues<ProjectTaskStatus>().Length);
    }

    [Fact]
    public async Task GenerateAsync_SeriesValuesSumToTotalTaskCount()
    {
        var tasks = new List<ProjectTask>
        {
            new() { Status = ProjectTaskStatus.Todo },
            new() { Status = ProjectTaskStatus.InProgress },
            new() { Status = ProjectTaskStatus.Done }
        };
        _taskRepo.Setup(r => r.GetAllForProjectAsync(1, default)).ReturnsAsync(tasks);

        var result = await _sut.GenerateAsync(new ReportRequest
        {
            Type = "tasks_by_status",
            Parameters = new ReportParameters { ProjectId = 1 }
        });

        result.Series.Should().HaveCount(1);
        result.Series[0].Data.Sum().Should().Be(3);
    }

    [Fact]
    public async Task GenerateAsync_ReportTypeIsCorrect()
    {
        _taskRepo.Setup(r => r.GetAllForProjectAsync(1, default)).ReturnsAsync(new List<ProjectTask>());

        var result = await _sut.GenerateAsync(new ReportRequest
        {
            Type = "tasks_by_status",
            Parameters = new ReportParameters { ProjectId = 1 }
        });

        result.ReportType.Should().Be("tasks_by_status");
        result.ProjectId.Should().Be(1);
        result.GeneratedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void ReportType_IsCorrectKey()
    {
        _sut.ReportType.Should().Be("tasks_by_status");
    }
}
