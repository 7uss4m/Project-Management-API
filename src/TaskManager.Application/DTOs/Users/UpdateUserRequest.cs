using System.ComponentModel.DataAnnotations;

namespace TaskManager.Application.DTOs.Users;

public class UpdateUserRequest
{
    [Required, MaxLength(200)]
    public string Name { get; init; } = string.Empty;

    [Required, EmailAddress, MaxLength(320)]
    public string Email { get; init; } = string.Empty;
}
