using System.ComponentModel.DataAnnotations;

namespace TaskManager.Application.DTOs.Projects;

public class AddMemberRequest
{
    [Required]
    public int UserId { get; init; }

    [MaxLength(100)]
    public string Role { get; init; } = "Member";
}
