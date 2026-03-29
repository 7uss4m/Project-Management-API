using TaskManager.Domain.Enums;

namespace TaskManager.Domain.Entities;

public class ProjectTask
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public Priority Priority { get; set; }
    public ProjectTaskStatus Status { get; set; }
    public DateTime? DueDate { get; set; }
    public int? AssigneeId { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsDeleted { get; set; }

    public Project Project { get; set; } = null!;
    public User? Assignee { get; set; }
}
