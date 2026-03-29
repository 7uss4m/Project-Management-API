namespace TaskManager.Application.Reports;

public class ReportRequest
{
    public string Type { get; init; } = string.Empty;
    public ReportParameters Parameters { get; init; } = new();
}

public class ReportParameters
{
    public int? ProjectId { get; init; }
    public DateTime? DateFrom { get; init; }
    public DateTime? DateTo { get; init; }
}
