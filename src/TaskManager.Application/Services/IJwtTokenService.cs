using TaskManager.Domain.Entities;

namespace TaskManager.Application.Services;

public interface IJwtTokenService
{
    string GenerateToken(User user);
}
