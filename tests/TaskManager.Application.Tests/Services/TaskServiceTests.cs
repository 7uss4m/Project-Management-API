using FluentAssertions;
using Moq;
using TaskManager.Application.DTOs.Tasks;
using TaskManager.Application.Services;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;
using TaskManager.Domain.Exceptions;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.Tests.Services;

public class TaskServiceTests
{
    private readonly Mock<ITaskRepository> _taskRepo = new();
    private readonly Mock<IProjectRepository> _projectRepo = new();
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly TaskService _sut;

    public TaskServiceTests()
    {
        _currentUser.Setup(c => c.UserId).Returns(1);
        _sut = new TaskService(_taskRepo.Object, _projectRepo.Object, _userRepo.Object, _currentUser.Object);
    }

    [Fact]
    public async Task CreateAsync_WithValidData_ReturnsCreatedTask()
    {
        var project = new Project { Id = 1, Name = "P" };
        _projectRepo.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync(project);
        _projectRepo.Setup(r => r.IsMemberAsync(1, 1, default)).ReturnsAsync(true);
        _taskRepo.Setup(r => r.AddAsync(It.IsAny<ProjectTask>(), default))
                 .ReturnsAsync((ProjectTask t, CancellationToken _) => { t.Id = 7; return t; });

        var result = await _sut.CreateAsync(1, new CreateTaskRequest
        {
            Title = "Fix bug",
            Priority = Priority.High
        });

        result.Id.Should().Be(7);
        result.Title.Should().Be("Fix bug");
        result.Priority.Should().Be(Priority.High);
    }

    [Fact]
    public async Task CreateAsync_WhenProjectNotFound_ThrowsNotFoundException()
    {
        _projectRepo.Setup(r => r.GetByIdAsync(99, default)).ReturnsAsync((Project?)null);

        await _sut.Invoking(s => s.CreateAsync(99, new CreateTaskRequest { Title = "X", Priority = Priority.Low }))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreateAsync_WithInvalidAssigneeId_ThrowsValidationException()
    {
        var project = new Project { Id = 1, Name = "P" };
        _projectRepo.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync(project);
        _projectRepo.Setup(r => r.IsMemberAsync(1, 1, default)).ReturnsAsync(true);
        _userRepo.Setup(r => r.ExistsAsync(99, default)).ReturnsAsync(false);

        await _sut.Invoking(s => s.CreateAsync(1, new CreateTaskRequest
            {
                Title = "X", Priority = Priority.Low, AssigneeId = 99
            }))
            .Should().ThrowAsync<ValidationException>()
            .WithMessage("*99*");
    }

    [Fact]
    public async Task GetByIdAsync_WhenTaskNotFound_ThrowsNotFoundException()
    {
        _taskRepo.Setup(r => r.GetByIdAsync(1, 99, default)).ReturnsAsync((ProjectTask?)null);

        await _sut.Invoking(s => s.GetByIdAsync(1, 99))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateStatusAsync_UpdatesAndReturnsTask()
    {
        var task = new ProjectTask { Id = 1, ProjectId = 1, Title = "T", Status = ProjectTaskStatus.Todo };
        _taskRepo.Setup(r => r.GetByIdAsync(1, 1, default)).ReturnsAsync(task);
        _projectRepo.Setup(r => r.IsMemberAsync(1, 1, default)).ReturnsAsync(true);
        _taskRepo.Setup(r => r.UpdateAsync(It.IsAny<ProjectTask>(), default))
                 .ReturnsAsync((ProjectTask t, CancellationToken _) => t);

        var result = await _sut.UpdateStatusAsync(1, 1, new UpdateTaskStatusRequest { Status = ProjectTaskStatus.InProgress });

        result.Status.Should().Be(ProjectTaskStatus.InProgress);
    }

    [Fact]
    public async Task GetPagedAsync_WithNonExistentAssigneeId_ThrowsValidationException()
    {
        var project = new Project { Id = 1, Name = "P" };
        _projectRepo.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync(project);
        _projectRepo.Setup(r => r.IsMemberAsync(1, 1, default)).ReturnsAsync(true);
        _userRepo.Setup(r => r.ExistsAsync(99, default)).ReturnsAsync(false);

        await _sut.Invoking(s => s.GetPagedAsync(1, 1, 20, null, null, assigneeId: 99))
            .Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task DeleteAsync_DelegatesToRepository()
    {
        var task = new ProjectTask { Id = 1, ProjectId = 1, Title = "T" };
        _taskRepo.Setup(r => r.GetByIdAsync(1, 1, default)).ReturnsAsync(task);
        _projectRepo.Setup(r => r.IsMemberAsync(1, 1, default)).ReturnsAsync(true);
        _taskRepo.Setup(r => r.DeleteAsync(1, 1, default)).Returns(Task.CompletedTask);

        await _sut.DeleteAsync(1, 1);

        _taskRepo.Verify(r => r.DeleteAsync(1, 1, default), Times.Once);
    }
}
