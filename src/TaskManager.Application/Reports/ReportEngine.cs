using TaskManager.Domain.Exceptions;

namespace TaskManager.Application.Reports;

public class ReportEngine
{
    private readonly IEnumerable<IReportGenerator> _generators;

    public ReportEngine(IEnumerable<IReportGenerator> generators) => _generators = generators;

    public Task<ReportResult> GenerateAsync(ReportRequest request, CancellationToken ct = default)
    {
        var generator = _generators.FirstOrDefault(g => g.ReportType == request.Type)
            ?? throw new ValidationException($"Report type '{request.Type}' is not supported.",
                new Dictionary<string, string[]> { ["type"] = [$"Unknown report type: {request.Type}"] });

        return generator.GenerateAsync(request, ct);
    }
}
