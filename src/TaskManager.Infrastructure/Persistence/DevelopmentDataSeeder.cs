using Microsoft.EntityFrameworkCore;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;

namespace TaskManager.Infrastructure.Persistence;

/// <summary>
/// Inserts a small demo dataset on first run in Development only (skipped if any user already exists).
/// </summary>
public static class DevelopmentDataSeeder
{
    public static async Task SeedAsync(AppDbContext db, CancellationToken ct = default)
    {
        if (await db.Users.AnyAsync(ct))
            return;

        var now = DateTime.UtcNow;

        var alex = new User
        {
            Name = "Alex Demo",
            Email = "demo@taskmanager.local",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Demo123!"),
            CreatedAt = now
        };
        var jane = new User
        {
            Name = "Jane Member",
            Email = "jane@taskmanager.local",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Demo123!"),
            CreatedAt = now
        };
        db.Users.AddRange(alex, jane);
        await db.SaveChangesAsync(ct);

        var project = new Project
        {
            Name = "Acme Website Redesign",
            Description = "Sample project seeded for local development.",
            CreatedAt = now,
            UpdatedAt = now
        };
        db.Projects.Add(project);
        await db.SaveChangesAsync(ct);

        db.ProjectMembers.AddRange(
            new ProjectMember
            {
                ProjectId = project.Id,
                UserId = alex.Id,
                Role = "Owner",
                JoinedAt = now
            },
            new ProjectMember
            {
                ProjectId = project.Id,
                UserId = jane.Id,
                Role = "Member",
                JoinedAt = now
            });

        db.Tasks.AddRange(
            new ProjectTask
            {
                ProjectId = project.Id,
                Title = "Draft homepage copy",
                Priority = Priority.High,
                Status = ProjectTaskStatus.Todo,
                DueDate = now.AddDays(14),
                AssigneeId = jane.Id,
                CreatedAt = new DateTime(2026, 1, 10, 12, 0, 0, DateTimeKind.Utc)
            },
            new ProjectTask
            {
                ProjectId = project.Id,
                Title = "Implement contact form API",
                Priority = Priority.Medium,
                Status = ProjectTaskStatus.InProgress,
                DueDate = now.AddDays(7),
                AssigneeId = jane.Id,
                CreatedAt = new DateTime(2026, 2, 5, 9, 0, 0, DateTimeKind.Utc)
            },
            new ProjectTask
            {
                ProjectId = project.Id,
                Title = "Accessibility audit",
                Priority = Priority.Low,
                Status = ProjectTaskStatus.Done,
                AssigneeId = alex.Id,
                CreatedAt = new DateTime(2026, 2, 20, 15, 30, 0, DateTimeKind.Utc)
            });

        await db.SaveChangesAsync(ct);
    }
}
