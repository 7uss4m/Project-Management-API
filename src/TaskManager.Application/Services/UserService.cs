using TaskManager.Application.DTOs.Common;
using TaskManager.Application.DTOs.Users;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Exceptions;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _users;

    public UserService(IUserRepository users) => _users = users;

    public async Task<PagedResult<UserDto>> GetPagedAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var (items, total) = await _users.GetPagedAsync(page, pageSize, ct);
        return PagedResult<UserDto>.From(items.Select(ToDto).ToList(), total, page, pageSize);
    }

    public async Task<UserDto> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var user = await _users.GetByIdAsync(id, ct)
            ?? throw NotFoundException.For<User>(id);
        return ToDto(user);
    }

    public async Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken ct = default)
    {
        var user = new User
        {
            Name = request.Name,
            Email = request.Email.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };
        var created = await _users.AddAsync(user, ct);
        return ToDto(created);
    }

    public async Task<UserDto> UpdateAsync(int id, UpdateUserRequest request, CancellationToken ct = default)
    {
        var user = await _users.GetByIdAsync(id, ct)
            ?? throw NotFoundException.For<User>(id);
        user.Name = request.Name;
        user.Email = request.Email;
        var updated = await _users.UpdateAsync(user, ct);
        return ToDto(updated);
    }

    public Task DeleteAsync(int id, CancellationToken ct = default) =>
        _users.DeleteAsync(id, ct);

    private static UserDto ToDto(User u) => new()
    {
        Id = u.Id,
        Name = u.Name,
        Email = u.Email,
        CreatedAt = u.CreatedAt
    };
}
