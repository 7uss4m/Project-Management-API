namespace TaskManager.Domain.Entities;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsDeleted { get; set; }

    public ICollection<ProjectMember> ProjectMemberships { get; set; } = [];
    public ICollection<Project> AssignedTasks { get; set; } = [];
}
