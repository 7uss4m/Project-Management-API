using System.ComponentModel.DataAnnotations;

namespace TaskManager.Application.DTOs.Auth;

public class RegisterRequest
{
    [Required, MaxLength(200)]
    public string Name { get; init; } = string.Empty;

    [Required, EmailAddress, MaxLength(320)]
    public string Email { get; init; } = string.Empty;

    [Required, MinLength(8), MaxLength(200)]
    public string Password { get; init; } = string.Empty;
}
