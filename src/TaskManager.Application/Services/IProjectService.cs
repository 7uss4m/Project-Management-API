using TaskManager.Application.DTOs.Common;
using TaskManager.Application.DTOs.Projects;

namespace TaskManager.Application.Services;

public interface IProjectService
{
    Task<PagedResult<ProjectDto>> GetPagedAsync(int page, int pageSize, CancellationToken ct = default);
    Task<ProjectDto> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ProjectDto> CreateAsync(CreateProjectRequest request, CancellationToken ct = default);
    Task<ProjectDto> UpdateAsync(int id, UpdateProjectRequest request, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
    Task AddMemberAsync(int projectId, AddMemberRequest request, CancellationToken ct = default);
    Task RemoveMemberAsync(int projectId, int userId, CancellationToken ct = default);
}
