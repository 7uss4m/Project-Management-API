using TaskManager.Domain.Entities;

namespace TaskManager.Domain.Interfaces;

public interface IProjectRepository
{
    Task<Project?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<(IReadOnlyList<Project> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, CancellationToken ct = default);
    Task<(IReadOnlyList<Project> Items, int TotalCount)> GetPagedForMemberUserAsync(
        int userId, int page, int pageSize, CancellationToken ct = default);
    Task<Project> AddAsync(Project project, CancellationToken ct = default);
    Task<Project> UpdateAsync(Project project, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
    Task<bool> IsMemberAsync(int projectId, int userId, CancellationToken ct = default);
    Task AddMemberAsync(ProjectMember member, CancellationToken ct = default);
    Task RemoveMemberAsync(int projectId, int userId, CancellationToken ct = default);
    Task<IQueryable<ProjectTask>> GetTasksQueryAsync(int projectId, CancellationToken ct = default);
}
