namespace TaskManager.Application.DTOs.Projects;

public class ProjectMemberDto
{
    public int UserId { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public DateTime JoinedAt { get; init; }
}
