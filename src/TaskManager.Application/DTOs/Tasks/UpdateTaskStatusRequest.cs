using System.ComponentModel.DataAnnotations;
using TaskManager.Domain.Enums;

namespace TaskManager.Application.DTOs.Tasks;

public class UpdateTaskStatusRequest
{
    [Required]
    public ProjectTaskStatus Status { get; init; }
}
