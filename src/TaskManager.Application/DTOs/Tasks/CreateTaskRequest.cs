using System.ComponentModel.DataAnnotations;
using TaskManager.Domain.Enums;

namespace TaskManager.Application.DTOs.Tasks;

public class CreateTaskRequest
{
    [Required, MaxLength(500)]
    public string Title { get; init; } = string.Empty;

    [Required]
    public Priority Priority { get; init; }

    public ProjectTaskStatus Status { get; init; } = ProjectTaskStatus.Todo;

    public DateTime? DueDate { get; init; }

    public int? AssigneeId { get; init; }
}
