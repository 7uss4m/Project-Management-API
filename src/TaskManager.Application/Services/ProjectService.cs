using TaskManager.Application.DTOs.Common;
using TaskManager.Application.DTOs.Projects;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Exceptions;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.Services;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projects;
    private readonly IUserRepository _users;
    private readonly ITaskRepository _tasks;
    private readonly ICurrentUserService _currentUser;

    public ProjectService(
        IProjectRepository projects,
        IUserRepository users,
        ITaskRepository tasks,
        ICurrentUserService currentUser)
    {
        _projects = projects;
        _users = users;
        _tasks = tasks;
        _currentUser = currentUser;
    }

    private int UserId => _currentUser.UserId;

    private async Task EnsureMemberAsync(int projectId, CancellationToken ct)
    {
        if (!await _projects.IsMemberAsync(projectId, UserId, ct))
            throw new ForbiddenException("You do not have access to this project.");
    }

    public async Task<PagedResult<ProjectDto>> GetPagedAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var (items, total) = await _projects.GetPagedForMemberUserAsync(UserId, page, pageSize, ct);
        var dtos = new List<ProjectDto>();
        foreach (var p in items)
        {
            var (taskItems, taskCount) = await _tasks.GetPagedAsync(p.Id, 1, int.MaxValue, null, null, null, ct);
            dtos.Add(ToDto(p, taskCount));
        }
        return PagedResult<ProjectDto>.From(dtos, total, page, pageSize);
    }

    public async Task<ProjectDto> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var project = await _projects.GetByIdAsync(id, ct)
            ?? throw NotFoundException.For<Project>(id);
        await EnsureMemberAsync(id, ct);
        var (_, taskCount) = await _tasks.GetPagedAsync(id, 1, int.MaxValue, null, null, null, ct);
        return ToDto(project, taskCount);
    }

    public async Task<ProjectDto> CreateAsync(CreateProjectRequest request, CancellationToken ct = default)
    {
        var project = new Project { Name = request.Name, Description = request.Description };
        var created = await _projects.AddAsync(project, ct);
        await _projects.AddMemberAsync(new ProjectMember
        {
            ProjectId = created.Id,
            UserId = UserId,
            Role = "Owner"
        }, ct);
        var reloaded = await _projects.GetByIdAsync(created.Id, ct) ?? created;
        return ToDto(reloaded, 0);
    }

    public async Task<ProjectDto> UpdateAsync(int id, UpdateProjectRequest request, CancellationToken ct = default)
    {
        var project = await _projects.GetByIdAsync(id, ct)
            ?? throw NotFoundException.For<Project>(id);
        await EnsureMemberAsync(id, ct);
        project.Name = request.Name;
        project.Description = request.Description;
        var updated = await _projects.UpdateAsync(project, ct);
        var (_, taskCount) = await _tasks.GetPagedAsync(id, 1, int.MaxValue, null, null, null, ct);
        return ToDto(updated, taskCount);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        _ = await _projects.GetByIdAsync(id, ct)
            ?? throw NotFoundException.For<Project>(id);
        await EnsureMemberAsync(id, ct);
        await _projects.DeleteAsync(id, ct);
    }

    public async Task AddMemberAsync(int projectId, AddMemberRequest request, CancellationToken ct = default)
    {
        var project = await _projects.GetByIdAsync(projectId, ct)
            ?? throw NotFoundException.For<Project>(projectId);
        await EnsureMemberAsync(projectId, ct);

        var userExists = await _users.ExistsAsync(request.UserId, ct);
        if (!userExists)
            throw NotFoundException.For<User>(request.UserId);

        var alreadyMember = await _projects.IsMemberAsync(projectId, request.UserId, ct);
        if (alreadyMember)
            throw new ConflictException($"User {request.UserId} is already a member of project {projectId}.");

        var member = new ProjectMember
        {
            ProjectId = projectId,
            UserId = request.UserId,
            Role = request.Role
        };
        await _projects.AddMemberAsync(member, ct);
    }

    public async Task RemoveMemberAsync(int projectId, int userId, CancellationToken ct = default)
    {
        _ = await _projects.GetByIdAsync(projectId, ct)
            ?? throw NotFoundException.For<Project>(projectId);
        await EnsureMemberAsync(projectId, ct);
        await _projects.RemoveMemberAsync(projectId, userId, ct);
    }

    private static ProjectDto ToDto(Project p, int taskCount) => new()
    {
        Id = p.Id,
        Name = p.Name,
        Description = p.Description,
        CreatedAt = p.CreatedAt,
        UpdatedAt = p.UpdatedAt,
        TaskCount = taskCount,
        Members = p.Members
            .Select(m => new ProjectMemberDto
            {
                UserId = m.UserId,
                UserName = m.User?.Name ?? string.Empty,
                Role = m.Role,
                JoinedAt = m.JoinedAt
            })
            .ToList()
    };
}
