using TaskManager.Domain.Enums;

namespace TaskManager.Application.DTOs.Tasks;

public class TaskDto
{
    public int Id { get; init; }
    public int ProjectId { get; init; }
    public string Title { get; init; } = string.Empty;
    public Priority Priority { get; init; }
    public ProjectTaskStatus Status { get; init; }
    public DateTime? DueDate { get; init; }
    public int? AssigneeId { get; init; }
    public string? AssigneeName { get; init; }
    public DateTime CreatedAt { get; init; }
}
