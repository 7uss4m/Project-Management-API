using System.Linq;
using TaskManager.Domain.Enums;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.Reports.Generators;

public class TasksByStatusReport : IReportGenerator
{
    private readonly ITaskRepository _tasks;

    public TasksByStatusReport(ITaskRepository tasks) => _tasks = tasks;

    public string ReportType => "tasks_by_status";

    public async Task<ReportResult> GenerateAsync(ReportRequest request, CancellationToken ct = default)
    {
        var projectId = request.Parameters.ProjectId
            ?? throw new ArgumentException("ProjectId is required for tasks_by_status report.");

        var allTasks = await _tasks.GetAllForProjectAsync(projectId, ct);

        var tasks = allTasks.AsEnumerable();
        var p = request.Parameters;
        if (p.DateFrom is { } from)
            tasks = tasks.Where(t => t.CreatedAt >= from);
        if (p.DateTo is { } to)
            tasks = tasks.Where(t => t.CreatedAt <= to);
        var filtered = tasks.ToList();

        var statuses = Enum.GetValues<ProjectTaskStatus>();
        var labels = statuses.Select(s => s.ToString()).ToArray();
        var data = statuses.Select(s => (decimal)filtered.Count(t => t.Status == s)).ToArray();

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
