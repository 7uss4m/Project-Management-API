using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManager.Domain.Entities;

namespace TaskManager.Infrastructure.Persistence.Configurations;

public class ProjectTaskConfiguration : IEntityTypeConfiguration<ProjectTask>
{
    public void Configure(EntityTypeBuilder<ProjectTask> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Title).IsRequired().HasMaxLength(500);
        builder.Property(t => t.Priority).HasConversion<string>();
        builder.Property(t => t.Status).HasConversion<string>();
        builder.HasQueryFilter(t => !t.IsDeleted);

        builder.HasOne(t => t.Assignee)
               .WithMany()
               .HasForeignKey(t => t.AssigneeId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}
