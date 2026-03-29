using TaskManager.Application.DTOs.Auth;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Exceptions;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthService(IUserRepository users, IJwtTokenService jwtTokenService)
    {
        _users = users;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var existing = await _users.GetByEmailAsync(request.Email, ct);
        if (existing is not null)
            throw new ConflictException($"An account with email '{request.Email}' already exists.");

        var user = new User
        {
            Name = request.Name,
            Email = request.Email.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        var created = await _users.AddAsync(user, ct);
        return ToResponse(created, _jwtTokenService.GenerateToken(created));
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await _users.GetByEmailAsync(request.Email, ct);
        if (user is null || string.IsNullOrEmpty(user.PasswordHash))
            throw new UnauthorizedException("Invalid email or password.");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedException("Invalid email or password.");

        return ToResponse(user, _jwtTokenService.GenerateToken(user));
    }

    private static AuthResponse ToResponse(User user, string token) => new()
    {
        Token = token,
        UserId = user.Id,
        Name = user.Name,
        Email = user.Email
    };
}
