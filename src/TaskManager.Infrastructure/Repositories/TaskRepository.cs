using Microsoft.EntityFrameworkCore;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;
using TaskManager.Domain.Exceptions;
using TaskManager.Domain.Interfaces;
using TaskManager.Infrastructure.Persistence;

namespace TaskManager.Infrastructure.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly AppDbContext _db;

    public TaskRepository(AppDbContext db) => _db = db;

    public async Task<ProjectTask?> GetByIdAsync(int projectId, int taskId, CancellationToken ct = default) =>
        await _db.Tasks
            .Include(t => t.Assignee)
            .FirstOrDefaultAsync(t => t.Id == taskId && t.ProjectId == projectId, ct);

    public async Task<(IReadOnlyList<ProjectTask> Items, int TotalCount)> GetPagedAsync(
        int projectId, int page, int pageSize,
        ProjectTaskStatus? status, Priority? priority, int? assigneeId,
        CancellationToken ct = default)
    {
        var query = _db.Tasks
            .Include(t => t.Assignee)
            .Where(t => t.ProjectId == projectId);

        if (status.HasValue)
            query = query.Where(t => t.Status == status.Value);
        if (priority.HasValue)
            query = query.Where(t => t.Priority == priority.Value);
        if (assigneeId.HasValue)
            query = query.Where(t => t.AssigneeId == assigneeId.Value);

        query = query.OrderBy(t => t.CreatedAt);
        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }

    public async Task<ProjectTask> AddAsync(ProjectTask task, CancellationToken ct = default)
    {
        task.CreatedAt = DateTime.UtcNow;
        _db.Tasks.Add(task);
        await _db.SaveChangesAsync(ct);
        return task;
    }

    public async Task<ProjectTask> UpdateAsync(ProjectTask task, CancellationToken ct = default)
    {
        _db.Tasks.Update(task);
        await _db.SaveChangesAsync(ct);
        return task;
    }

    public async Task DeleteAsync(int projectId, int taskId, CancellationToken ct = default)
    {
        var task = await _db.Tasks
            .FirstOrDefaultAsync(t => t.Id == taskId && t.ProjectId == projectId, ct)
            ?? throw new NotFoundException($"Task with id {taskId} not found in project {projectId}.");
        task.IsDeleted = true;
        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<ProjectTask>> GetAllForProjectAsync(int projectId, CancellationToken ct = default) =>
        await _db.Tasks.Where(t => t.ProjectId == projectId).ToListAsync(ct);
}
