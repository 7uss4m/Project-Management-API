using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.DTOs.Tasks;
using TaskManager.Application.Services;
using TaskManager.Domain.Enums;
using TaskManager.Domain.Exceptions;

namespace TaskManager.API.Controllers;

[ApiController]
[Route("projects/{projectId:int}/tasks")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService) => _taskService = taskService;

    [HttpGet]
    public async Task<IActionResult> GetAll(
        int projectId,
        [FromQuery] string? status,
        [FromQuery] string? priority,
        [FromQuery] int? assigneeId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        ProjectTaskStatus? parsedStatus = null;
        if (status is not null)
        {
            if (!Enum.TryParse<ProjectTaskStatus>(status, ignoreCase: true, out var s))
                throw new ValidationException($"Invalid status value '{status}'.",
                    new Dictionary<string, string[]> { ["status"] = [$"'{status}' is not a valid status."] });
            parsedStatus = s;
        }

        Priority? parsedPriority = null;
        if (priority is not null)
        {
            if (!Enum.TryParse<Priority>(priority, ignoreCase: true, out var p))
                throw new ValidationException($"Invalid priority value '{priority}'.",
                    new Dictionary<string, string[]> { ["priority"] = [$"'{priority}' is not a valid priority."] });
            parsedPriority = p;
        }

        var result = await _taskService.GetPagedAsync(projectId, page, pageSize, parsedStatus, parsedPriority, assigneeId, ct);
        return Ok(result);
    }

    [HttpGet("{taskId:int}")]
    public async Task<IActionResult> GetById(int projectId, int taskId, CancellationToken ct = default)
    {
        var task = await _taskService.GetByIdAsync(projectId, taskId, ct);
        return Ok(task);
    }

    [HttpPost]
    public async Task<IActionResult> Create(int projectId, [FromBody] CreateTaskRequest request, CancellationToken ct = default)
    {
        var task = await _taskService.CreateAsync(projectId, request, ct);
        return CreatedAtAction(nameof(GetById), new { projectId, taskId = task.Id }, task);
    }

    [HttpPatch("{taskId:int}")]
    public async Task<IActionResult> Update(int projectId, int taskId, [FromBody] UpdateTaskRequest request, CancellationToken ct = default)
    {
        var task = await _taskService.UpdateAsync(projectId, taskId, request, ct);
        return Ok(task);
    }

    [HttpPatch("{taskId:int}/status")]
    public async Task<IActionResult> UpdateStatus(int projectId, int taskId, [FromBody] UpdateTaskStatusRequest request, CancellationToken ct = default)
    {
        var task = await _taskService.UpdateStatusAsync(projectId, taskId, request, ct);
        return Ok(task);
    }

    [HttpPatch("{taskId:int}/assignee")]
    public async Task<IActionResult> UpdateAssignee(int projectId, int taskId, [FromBody] UpdateTaskAssigneeRequest request, CancellationToken ct = default)
    {
        var task = await _taskService.UpdateAssigneeAsync(projectId, taskId, request, ct);
        return Ok(task);
    }

    [HttpDelete("{taskId:int}")]
    public async Task<IActionResult> Delete(int projectId, int taskId, CancellationToken ct = default)
    {
        await _taskService.DeleteAsync(projectId, taskId, ct);
        return NoContent();
    }
}
