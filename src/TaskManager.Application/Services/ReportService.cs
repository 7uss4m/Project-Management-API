using TaskManager.Application.Reports;
using TaskManager.Domain.Exceptions;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.Services;

public class ReportService : IReportService
{
    private readonly ReportEngine _reportEngine;
    private readonly IProjectRepository _projects;
    private readonly ICurrentUserService _currentUser;

    public ReportService(
        ReportEngine reportEngine,
        IProjectRepository projects,
        ICurrentUserService currentUser)
    {
        _reportEngine = reportEngine;
        _projects = projects;
        _currentUser = currentUser;
    }

    public async Task<ReportResult> GenerateAsync(ReportRequest request, CancellationToken ct = default)
    {
        if (request.Parameters.ProjectId is { } projectId)
        {
            var isMember = await _projects.IsMemberAsync(projectId, _currentUser.UserId, ct);
            if (!isMember)
                throw new ForbiddenException("You do not have access to this project.");
        }

        return await _reportEngine.GenerateAsync(request, ct);
    }
}
