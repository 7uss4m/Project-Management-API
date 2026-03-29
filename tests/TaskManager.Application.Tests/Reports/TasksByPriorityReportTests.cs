using FluentAssertions;
using Moq;
using TaskManager.Application.Reports;
using TaskManager.Application.Reports.Generators;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.Tests.Reports;

public class TasksByPriorityReportTests
{
    private readonly Mock<ITaskRepository> _taskRepo = new();
    private readonly TasksByPriorityReport _sut;

    public TasksByPriorityReportTests() => _sut = new TasksByPriorityReport(_taskRepo.Object);

    [Fact]
    public async Task GenerateAsync_ReturnsAllPrioritiesAsLabels()
    {
        _taskRepo.Setup(r => r.GetAllForProjectAsync(1, default)).ReturnsAsync(new List<ProjectTask>());

        var result = await _sut.GenerateAsync(new ReportRequest
        {
            Type = "tasks_by_priority",
            Parameters = new ReportParameters { ProjectId = 1 }
        });

        result.Labels.Should().Contain(nameof(Priority.Low));
        result.Labels.Should().Contain(nameof(Priority.High));
        result.Labels.Should().HaveCount(Enum.GetValues<Priority>().Length);
        result.ProjectId.Should().Be(1);
    }

    [Fact]
    public async Task GenerateAsync_SeriesValuesSumToTotalTaskCount()
    {
        var tasks = new List<ProjectTask>
        {
            new() { Priority = Priority.High },
            new() { Priority = Priority.High },
            new() { Priority = Priority.Low }
        };
        _taskRepo.Setup(r => r.GetAllForProjectAsync(1, default)).ReturnsAsync(tasks);

        var result = await _sut.GenerateAsync(new ReportRequest
        {
            Type = "tasks_by_priority",
            Parameters = new ReportParameters { ProjectId = 1 }
        });

        result.Series[0].Data.Sum().Should().Be(3);
    }

    [Fact]
    public void ReportType_IsCorrectKey()
    {
        _sut.ReportType.Should().Be("tasks_by_priority");
    }
}
