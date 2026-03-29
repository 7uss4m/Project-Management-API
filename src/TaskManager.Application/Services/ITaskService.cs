using TaskManager.Application.DTOs.Common;
using TaskManager.Application.DTOs.Tasks;
using TaskManager.Domain.Enums;

namespace TaskManager.Application.Services;

public interface ITaskService
{
    Task<PagedResult<TaskDto>> GetPagedAsync(
        int projectId, int page, int pageSize,
        ProjectTaskStatus? status, Priority? priority, int? assigneeId,
        CancellationToken ct = default);
    Task<TaskDto> GetByIdAsync(int projectId, int taskId, CancellationToken ct = default);
    Task<TaskDto> CreateAsync(int projectId, CreateTaskRequest request, CancellationToken ct = default);
    Task<TaskDto> UpdateAsync(int projectId, int taskId, UpdateTaskRequest request, CancellationToken ct = default);
    Task<TaskDto> UpdateStatusAsync(int projectId, int taskId, UpdateTaskStatusRequest request, CancellationToken ct = default);
    Task<TaskDto> UpdateAssigneeAsync(int projectId, int taskId, UpdateTaskAssigneeRequest request, CancellationToken ct = default);
    Task DeleteAsync(int projectId, int taskId, CancellationToken ct = default);
}
