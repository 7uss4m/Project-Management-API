using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.Reports;
using TaskManager.Application.Services;

namespace TaskManager.API.Controllers;

[ApiController]
[Route("reports")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService) => _reportService = reportService;

    [HttpPost]
    public async Task<IActionResult> Generate([FromBody] ReportRequest request, CancellationToken ct = default)
    {
        var result = await _reportService.GenerateAsync(request, ct);
        return Ok(result);
    }
}
