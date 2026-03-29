using System.ComponentModel.DataAnnotations;
using TaskManager.Domain.Enums;

namespace TaskManager.Application.DTOs.Tasks;

public class UpdateTaskRequest
{
    [MaxLength(500)]
    public string? Title { get; init; }

    public Priority? Priority { get; init; }

    public ProjectTaskStatus? Status { get; init; }

    public DateTime? DueDate { get; init; }

    public int? AssigneeId { get; init; }
}
