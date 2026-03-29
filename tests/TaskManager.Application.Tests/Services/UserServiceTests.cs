using FluentAssertions;
using Moq;
using TaskManager.Application.DTOs.Users;
using TaskManager.Application.Services;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Exceptions;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.Tests.Services;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _repo = new();
    private readonly UserService _sut;

    public UserServiceTests() => _sut = new UserService(_repo.Object);

    [Fact]
    public async Task GetByIdAsync_WhenUserExists_ReturnsDto()
    {
        var user = new User { Id = 1, Name = "Alice", Email = "alice@example.com", CreatedAt = DateTime.UtcNow };
        _repo.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync(user);

        var result = await _sut.GetByIdAsync(1);

        result.Id.Should().Be(1);
        result.Name.Should().Be("Alice");
    }

    [Fact]
    public async Task GetByIdAsync_WhenUserNotFound_ThrowsNotFoundException()
    {
        _repo.Setup(r => r.GetByIdAsync(99, default)).ReturnsAsync((User?)null);

        await _sut.Invoking(s => s.GetByIdAsync(99))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreateAsync_WithValidData_ReturnsCreatedUser()
    {
        const string plainPassword = "BobSecret1!";
        var request = new CreateUserRequest { Name = "Bob", Email = "bob@example.com", Password = plainPassword };
        _repo.Setup(r => r.AddAsync(It.IsAny<User>(), default))
             .ReturnsAsync((User u, CancellationToken _) => { u.Id = 5; return u; });

        var result = await _sut.CreateAsync(request);

        result.Id.Should().Be(5);
        result.Name.Should().Be("Bob");
        _repo.Verify(r => r.AddAsync(It.Is<User>(u =>
            u.Name == "Bob" && BCrypt.Net.BCrypt.Verify(plainPassword, u.PasswordHash)), default), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenUserNotFound_ThrowsNotFoundException()
    {
        _repo.Setup(r => r.GetByIdAsync(99, default)).ReturnsAsync((User?)null);

        await _sut.Invoking(s => s.UpdateAsync(99, new UpdateUserRequest { Name = "X", Email = "x@x.com" }))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task DeleteAsync_DelegatesToRepository()
    {
        _repo.Setup(r => r.DeleteAsync(1, default)).Returns(Task.CompletedTask);

        await _sut.DeleteAsync(1);

        _repo.Verify(r => r.DeleteAsync(1, default), Times.Once);
    }

    [Fact]
    public async Task GetPagedAsync_ReturnsPaginatedResult()
    {
        var users = new List<User>
        {
            new() { Id = 1, Name = "Alice", Email = "a@a.com" },
            new() { Id = 2, Name = "Bob", Email = "b@b.com" }
        };
        _repo.Setup(r => r.GetPagedAsync(1, 20, default)).ReturnsAsync((users, 2));

        var result = await _sut.GetPagedAsync(1, 20);

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(20);
    }
}
