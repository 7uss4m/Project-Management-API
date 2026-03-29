using System.Linq;
using TaskManager.Domain.Enums;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.Reports.Generators;

public class TasksByPriorityReport : IReportGenerator
{
    private readonly ITaskRepository _tasks;

    public TasksByPriorityReport(ITaskRepository tasks) => _tasks = tasks;

    public string ReportType => "tasks_by_priority";

    public async Task<ReportResult> GenerateAsync(ReportRequest request, CancellationToken ct = default)
    {
        var projectId = request.Parameters.ProjectId
            ?? throw new ArgumentException("ProjectId is required for tasks_by_priority report.");

        var allTasks = await _tasks.GetAllForProjectAsync(projectId, ct);

        var tasks = allTasks.AsEnumerable();
        var p = request.Parameters;
        if (p.DateFrom is { } from)
            tasks = tasks.Where(t => t.CreatedAt >= from);
        if (p.DateTo is { } to)
            tasks = tasks.Where(t => t.CreatedAt <= to);
        var filtered = tasks.ToList();

        var priorities = Enum.GetValues<Priority>();
        var labels = priorities.Select(p => p.ToString()).ToArray();
        var data = priorities.Select(p => (decimal)filtered.Count(t => t.Priority == p)).ToArray();

        return new ReportResult
        {
            ReportType = ReportType,
            ProjectId = projectId,
            GeneratedAt = DateTime.UtcNow,
            Labels = labels,
            Series = [new ReportSeries { Name = "Task Count", Data = data }]
        };
    }
}
