namespace TaskManager.Application.Reports;

public class ReportResult
{
    public string ReportType { get; init; } = string.Empty;
    /// <summary>Project the report was generated for, when applicable.</summary>
    public int? ProjectId { get; init; }
    public DateTime GeneratedAt { get; init; }
    public string[] Labels { get; init; } = [];
    public ReportSeries[] Series { get; init; } = [];
}

public class ReportSeries
{
    public string Name { get; init; } = string.Empty;
    public decimal[] Data { get; init; } = [];
}
