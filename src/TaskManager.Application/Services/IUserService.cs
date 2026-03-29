using TaskManager.Application.DTOs.Common;
using TaskManager.Application.DTOs.Users;

namespace TaskManager.Application.Services;

public interface IUserService
{
    Task<PagedResult<UserDto>> GetPagedAsync(int page, int pageSize, CancellationToken ct = default);
    Task<UserDto> GetByIdAsync(int id, CancellationToken ct = default);
    Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken ct = default);
    Task<UserDto> UpdateAsync(int id, UpdateUserRequest request, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
