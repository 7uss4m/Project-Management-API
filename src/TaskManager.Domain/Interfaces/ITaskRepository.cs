using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;

namespace TaskManager.Domain.Interfaces;

public interface ITaskRepository
{
    Task<ProjectTask?> GetByIdAsync(int projectId, int taskId, CancellationToken ct = default);
    Task<(IReadOnlyList<ProjectTask> Items, int TotalCount)> GetPagedAsync(
        int projectId,
        int page,
        int pageSize,
        ProjectTaskStatus? status,
        Priority? priority,
        int? assigneeId,
        CancellationToken ct = default);
    Task<ProjectTask> AddAsync(ProjectTask task, CancellationToken ct = default);
    Task<ProjectTask> UpdateAsync(ProjectTask task, CancellationToken ct = default);
    Task DeleteAsync(int projectId, int taskId, CancellationToken ct = default);
    Task<IReadOnlyList<ProjectTask>> GetAllForProjectAsync(int projectId, CancellationToken ct = default);
}
