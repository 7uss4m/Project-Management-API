using FluentAssertions;
using Moq;
using TaskManager.Application.DTOs.Projects;
using TaskManager.Application.Services;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Exceptions;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.Tests.Services;

public class ProjectServiceTests
{
    private readonly Mock<IProjectRepository> _projectRepo = new();
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<ITaskRepository> _taskRepo = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly ProjectService _sut;

    public ProjectServiceTests()
    {
        _currentUser.Setup(c => c.UserId).Returns(1);
        _sut = new ProjectService(_projectRepo.Object, _userRepo.Object, _taskRepo.Object, _currentUser.Object);
    }

    [Fact]
    public async Task GetByIdAsync_WhenProjectExists_ReturnsDto()
    {
        var project = new Project { Id = 1, Name = "Alpha", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        _projectRepo.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync(project);
        _projectRepo.Setup(r => r.IsMemberAsync(1, 1, default)).ReturnsAsync(true);
        _taskRepo.Setup(r => r.GetPagedAsync(1, 1, int.MaxValue, null, null, null, default))
                 .ReturnsAsync((new List<ProjectTask>(), 3));

        var result = await _sut.GetByIdAsync(1);

        result.Id.Should().Be(1);
        result.Name.Should().Be("Alpha");
        result.TaskCount.Should().Be(3);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ThrowsNotFoundException()
    {
        _projectRepo.Setup(r => r.GetByIdAsync(99, default)).ReturnsAsync((Project?)null);

        await _sut.Invoking(s => s.GetByIdAsync(99))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreateAsync_WithValidData_ReturnsCreatedProject()
    {
        var request = new CreateProjectRequest { Name = "Beta" };
        _projectRepo.Setup(r => r.AddAsync(It.IsAny<Project>(), default))
                    .ReturnsAsync((Project p, CancellationToken _) => { p.Id = 10; return p; });
        _projectRepo.Setup(r => r.AddMemberAsync(It.IsAny<ProjectMember>(), default)).Returns(Task.CompletedTask);
        var reloaded = new Project
        {
            Id = 10,
            Name = "Beta",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Members = []
        };
        _projectRepo.Setup(r => r.GetByIdAsync(10, default)).ReturnsAsync(reloaded);

        var result = await _sut.CreateAsync(request);

        result.Id.Should().Be(10);
        result.Name.Should().Be("Beta");
        _projectRepo.Verify(r => r.AddMemberAsync(It.Is<ProjectMember>(m =>
            m.ProjectId == 10 && m.UserId == 1 && m.Role == "Owner"), default), Times.Once);
    }

    [Fact]
    public async Task AddMemberAsync_WhenAlreadyMember_ThrowsConflictException()
    {
        var project = new Project { Id = 1, Name = "X" };
        _projectRepo.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync(project);
        _projectRepo.Setup(r => r.IsMemberAsync(1, 1, default)).ReturnsAsync(true);
        _userRepo.Setup(r => r.ExistsAsync(5, default)).ReturnsAsync(true);
        _projectRepo.Setup(r => r.IsMemberAsync(1, 5, default)).ReturnsAsync(true);

        await _sut.Invoking(s => s.AddMemberAsync(1, new AddMemberRequest { UserId = 5 }))
            .Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task AddMemberAsync_WhenUserNotFound_ThrowsNotFoundException()
    {
        var project = new Project { Id = 1, Name = "X" };
        _projectRepo.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync(project);
        _projectRepo.Setup(r => r.IsMemberAsync(1, 1, default)).ReturnsAsync(true);
        _userRepo.Setup(r => r.ExistsAsync(99, default)).ReturnsAsync(false);

        await _sut.Invoking(s => s.AddMemberAsync(1, new AddMemberRequest { UserId = 99 }))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task DeleteAsync_DelegatesToRepository()
    {
        var project = new Project { Id = 1, Name = "X" };
        _projectRepo.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync(project);
        _projectRepo.Setup(r => r.IsMemberAsync(1, 1, default)).ReturnsAsync(true);
        _projectRepo.Setup(r => r.DeleteAsync(1, default)).Returns(Task.CompletedTask);

        await _sut.DeleteAsync(1);

        _projectRepo.Verify(r => r.DeleteAsync(1, default), Times.Once);
    }
}
