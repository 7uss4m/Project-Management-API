using System.ComponentModel.DataAnnotations;

namespace TaskManager.Application.DTOs.Auth;

public class LoginRequest
{
    [Required, EmailAddress, MaxLength(320)]
    public string Email { get; init; } = string.Empty;

    [Required]
    public string Password { get; init; } = string.Empty;
}
