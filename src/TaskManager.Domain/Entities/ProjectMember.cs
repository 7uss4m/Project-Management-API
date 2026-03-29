namespace TaskManager.Domain.Entities;

public class ProjectMember
{
    public int ProjectId { get; set; }
    public int UserId { get; set; }
    public string Role { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; }
    public bool IsDeleted { get; set; }

    public Project Project { get; set; } = null!;
    public User User { get; set; } = null!;
}
