namespace TaskManager.Application.DTOs.Projects;

public class ProjectDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public int TaskCount { get; init; }
    public IReadOnlyList<ProjectMemberDto> Members { get; init; } = [];
}
