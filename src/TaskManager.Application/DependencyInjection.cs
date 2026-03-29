using Microsoft.Extensions.DependencyInjection;
using TaskManager.Application.Reports;
using TaskManager.Application.Reports.Generators;
using TaskManager.Application.Services;

namespace TaskManager.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<ITaskService, TaskService>();
        services.AddScoped<IAuthService, AuthService>();

        services.AddScoped<IReportGenerator, TasksByStatusReport>();
        services.AddScoped<IReportGenerator, TasksByPriorityReport>();
        services.AddScoped<ReportEngine>();
        services.AddScoped<IReportService, ReportService>();

        return services;
    }
}
