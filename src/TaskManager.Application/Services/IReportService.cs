using TaskManager.Application.Reports;

namespace TaskManager.Application.Services;

public interface IReportService
{
    Task<ReportResult> GenerateAsync(ReportRequest request, CancellationToken ct = default);
}
