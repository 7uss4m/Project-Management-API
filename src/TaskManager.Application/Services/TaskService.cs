using TaskManager.Application.DTOs.Common;
using TaskManager.Application.DTOs.Tasks;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;
using TaskManager.Domain.Exceptions;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _tasks;
    private readonly IProjectRepository _projects;
    private readonly IUserRepository _users;
    private readonly ICurrentUserService _currentUser;

    public TaskService(
        ITaskRepository tasks,
        IProjectRepository projects,
        IUserRepository users,
        ICurrentUserService currentUser)
    {
        _tasks = tasks;
        _projects = projects;
        _users = users;
        _currentUser = currentUser;
    }

    private int UserId => _currentUser.UserId;

    private async Task EnsureProjectMemberAsync(int projectId, CancellationToken ct)
    {
        if (!await _projects.IsMemberAsync(projectId, UserId, ct))
            throw new ForbiddenException("You do not have access to this project.");
    }

    private async Task EnsureTaskListAccessAsync(int projectId, int? assigneeFilter, CancellationToken ct)
    {
        if (await _projects.IsMemberAsync(projectId, UserId, ct))
            return;
        if (assigneeFilter == UserId)
            return;
        throw new ForbiddenException("You do not have access to tasks in this project.");
    }

    private async Task EnsureMemberOrAssigneeAsync(ProjectTask task, CancellationToken ct)
    {
        if (await _projects.IsMemberAsync(task.ProjectId, UserId, ct))
            return;
        if (task.AssigneeId == UserId)
            return;
        throw new ForbiddenException("You do not have access to this task.");
    }

    public async Task<PagedResult<TaskDto>> GetPagedAsync(
        int projectId, int page, int pageSize,
        ProjectTaskStatus? status, Priority? priority, int? assigneeId,
        CancellationToken ct = default)
    {
        _ = await _projects.GetByIdAsync(projectId, ct)
            ?? throw NotFoundException.For<Project>(projectId);

        await EnsureTaskListAccessAsync(projectId, assigneeId, ct);

        if (assigneeId.HasValue && !await _users.ExistsAsync(assigneeId.Value, ct))
            throw new ValidationException($"User with id {assigneeId.Value} does not exist.",
                new Dictionary<string, string[]> { ["assigneeId"] = [$"User {assigneeId.Value} not found."] });

        var (items, total) = await _tasks.GetPagedAsync(projectId, page, pageSize, status, priority, assigneeId, ct);
        return PagedResult<TaskDto>.From(items.Select(ToDto).ToList(), total, page, pageSize);
    }

    public async Task<TaskDto> GetByIdAsync(int projectId, int taskId, CancellationToken ct = default)
    {
        var task = await _tasks.GetByIdAsync(projectId, taskId, ct)
            ?? throw new NotFoundException($"Task with id {taskId} not found in project {projectId}.");
        await EnsureMemberOrAssigneeAsync(task, ct);
        return ToDto(task);
    }

    public async Task<TaskDto> CreateAsync(int projectId, CreateTaskRequest request, CancellationToken ct = default)
    {
        _ = await _projects.GetByIdAsync(projectId, ct)
            ?? throw NotFoundException.For<Project>(projectId);

        await EnsureProjectMemberAsync(projectId, ct);

        if (request.AssigneeId.HasValue && !await _users.ExistsAsync(request.AssigneeId.Value, ct))
            throw new ValidationException($"Assignee with id {request.AssigneeId.Value} does not exist.",
                new Dictionary<string, string[]> { ["assigneeId"] = [$"User {request.AssigneeId.Value} not found."] });

        var task = new ProjectTask
        {
            ProjectId = projectId,
            Title = request.Title,
            Priority = request.Priority,
            Status = request.Status,
            DueDate = request.DueDate,
            AssigneeId = request.AssigneeId
        };
        var created = await _tasks.AddAsync(task, ct);
        return ToDto(created);
    }

    public async Task<TaskDto> UpdateAsync(int projectId, int taskId, UpdateTaskRequest request, CancellationToken ct = default)
    {
        var task = await _tasks.GetByIdAsync(projectId, taskId, ct)
            ?? throw new NotFoundException($"Task with id {taskId} not found in project {projectId}.");
        await EnsureMemberOrAssigneeAsync(task, ct);

        if (request.AssigneeId.HasValue && !await _users.ExistsAsync(request.AssigneeId.Value, ct))
            throw new ValidationException($"Assignee with id {request.AssigneeId.Value} does not exist.",
                new Dictionary<string, string[]> { ["assigneeId"] = [$"User {request.AssigneeId.Value} not found."] });

        if (request.Title is not null) task.Title = request.Title;
        if (request.Priority.HasValue) task.Priority = request.Priority.Value;
        if (request.Status.HasValue) task.Status = request.Status.Value;
        if (request.DueDate.HasValue) task.DueDate = request.DueDate.Value;
        if (request.AssigneeId is not null) task.AssigneeId = request.AssigneeId == 0 ? null : request.AssigneeId;

        var updated = await _tasks.UpdateAsync(task, ct);
        return ToDto(updated);
    }

    public async Task<TaskDto> UpdateStatusAsync(int projectId, int taskId, UpdateTaskStatusRequest request, CancellationToken ct = default)
    {
        var task = await _tasks.GetByIdAsync(projectId, taskId, ct)
            ?? throw new NotFoundException($"Task with id {taskId} not found in project {projectId}.");
        await EnsureMemberOrAssigneeAsync(task, ct);
        task.Status = request.Status;
        var updated = await _tasks.UpdateAsync(task, ct);
        return ToDto(updated);
    }

    public async Task<TaskDto> UpdateAssigneeAsync(int projectId, int taskId, UpdateTaskAssigneeRequest request, CancellationToken ct = default)
    {
        var task = await _tasks.GetByIdAsync(projectId, taskId, ct)
            ?? throw new NotFoundException($"Task with id {taskId} not found in project {projectId}.");
        await EnsureMemberOrAssigneeAsync(task, ct);

        if (request.AssigneeId.HasValue && !await _users.ExistsAsync(request.AssigneeId.Value, ct))
            throw new ValidationException($"Assignee with id {request.AssigneeId.Value} does not exist.",
                new Dictionary<string, string[]> { ["assigneeId"] = [$"User {request.AssigneeId.Value} not found."] });

        task.AssigneeId = request.AssigneeId;
        var updated = await _tasks.UpdateAsync(task, ct);
        return ToDto(updated);
    }

    public async Task DeleteAsync(int projectId, int taskId, CancellationToken ct = default)
    {
        var task = await _tasks.GetByIdAsync(projectId, taskId, ct)
            ?? throw new NotFoundException($"Task with id {taskId} not found in project {projectId}.");
        await EnsureMemberOrAssigneeAsync(task, ct);
        await _tasks.DeleteAsync(projectId, taskId, ct);
    }

    private static TaskDto ToDto(ProjectTask t) => new()
    {
        Id = t.Id,
        ProjectId = t.ProjectId,
        Title = t.Title,
        Priority = t.Priority,
        Status = t.Status,
        DueDate = t.DueDate,
        AssigneeId = t.AssigneeId,
        AssigneeName = t.Assignee?.Name,
        CreatedAt = t.CreatedAt
    };
}
