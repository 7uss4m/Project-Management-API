using Microsoft.EntityFrameworkCore;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Exceptions;
using TaskManager.Domain.Interfaces;
using TaskManager.Infrastructure.Persistence;

namespace TaskManager.Infrastructure.Repositories;

public class ProjectRepository : IProjectRepository
{
    private readonly AppDbContext _db;

    public ProjectRepository(AppDbContext db) => _db = db;

    public async Task<Project?> GetByIdAsync(int id, CancellationToken ct = default) =>
        await _db.Projects
            .Include(p => p.Members).ThenInclude(m => m.User)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<(IReadOnlyList<Project> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, CancellationToken ct = default)
    {
        var query = _db.Projects.OrderBy(p => p.Name);
        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
        return (items, total);
    }

    public async Task<(IReadOnlyList<Project> Items, int TotalCount)> GetPagedForMemberUserAsync(
        int userId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = _db.Projects
            .Where(p => p.Members.Any(m => m.UserId == userId))
            .Include(p => p.Members).ThenInclude(m => m.User)
            .OrderBy(p => p.Name);
        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
        return (items, total);
    }

    public async Task<Project> AddAsync(Project project, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        project.CreatedAt = now;
        project.UpdatedAt = now;
        _db.Projects.Add(project);
        await _db.SaveChangesAsync(ct);
        return project;
    }

    public async Task<Project> UpdateAsync(Project project, CancellationToken ct = default)
    {
        project.UpdatedAt = DateTime.UtcNow;
        _db.Projects.Update(project);
        await _db.SaveChangesAsync(ct);
        return project;
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var project = await _db.Projects
            .Include(p => p.Tasks)
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw NotFoundException.For<Project>(id);

        var now = DateTime.UtcNow;
        project.IsDeleted = true;
        project.UpdatedAt = now;

        foreach (var task in project.Tasks)
            task.IsDeleted = true;

        foreach (var member in project.Members)
            member.IsDeleted = true;

        await _db.SaveChangesAsync(ct);
    }

    public async Task<bool> IsMemberAsync(int projectId, int userId, CancellationToken ct = default) =>
        await _db.ProjectMembers.AnyAsync(m => m.ProjectId == projectId && m.UserId == userId, ct);

    public async Task AddMemberAsync(ProjectMember member, CancellationToken ct = default)
    {
        member.JoinedAt = DateTime.UtcNow;
        _db.ProjectMembers.Add(member);
        await _db.SaveChangesAsync(ct);
    }

    public async Task RemoveMemberAsync(int projectId, int userId, CancellationToken ct = default)
    {
        var member = await _db.ProjectMembers
            .FirstOrDefaultAsync(m => m.ProjectId == projectId && m.UserId == userId, ct)
            ?? throw new NotFoundException($"Member with userId {userId} not found in project {projectId}.");
        member.IsDeleted = true;
        await _db.SaveChangesAsync(ct);
    }

    public Task<IQueryable<ProjectTask>> GetTasksQueryAsync(int projectId, CancellationToken ct = default)
    {
        IQueryable<ProjectTask> query = _db.Tasks
            .Where(t => t.ProjectId == projectId)
            .Include(t => t.Assignee);
        return Task.FromResult(query);
    }
}
