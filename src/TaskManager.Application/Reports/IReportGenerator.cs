namespace TaskManager.Application.Reports;

public interface IReportGenerator
{
    string ReportType { get; }
    Task<ReportResult> GenerateAsync(ReportRequest request, CancellationToken ct = default);
}
