using System.ComponentModel.DataAnnotations;

namespace TaskManager.Application.DTOs.Projects;

public class CreateProjectRequest
{
    [Required, MaxLength(200)]
    public string Name { get; init; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; init; }
}
