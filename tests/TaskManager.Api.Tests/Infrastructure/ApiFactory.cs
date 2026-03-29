using System.Net.Http.Headers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskManager.Application.Services;
using TaskManager.Domain.Entities;
using TaskManager.Infrastructure.Persistence;

namespace TaskManager.Api.Tests.Infrastructure;

public class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string SeedUserEmail = "test@local.test";
    public const string SeedUserPassword = "Test123!";

    private readonly string _dbName = $"test_{Guid.NewGuid()}.db";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite(
                    $"Data Source={_dbName}",
                    sqliteOptions => sqliteOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));
        });

        builder.UseEnvironment("Testing");
    }

    public async Task InitializeAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();

        if (!await db.Users.AnyAsync(u => u.Email == SeedUserEmail))
        {
            db.Users.Add(new User
            {
                Name = "Test User",
                Email = SeedUserEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(SeedUserPassword),
                CreatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }
    }

    /// <summary>
    /// HTTP client with Bearer JWT. Defaults to the seeded user (<see cref="SeedUserEmail"/>).
    /// </summary>
    public HttpClient CreateAuthenticatedClient(int? userId = null)
    {
        var client = CreateClient();
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = userId is int id
            ? db.Users.FirstOrDefault(u => u.Id == id)
            : db.Users.FirstOrDefault(u => u.Email == SeedUserEmail);
        if (user is null)
            throw new InvalidOperationException("User for JWT not found. Ensure tests seed or create the user first.");

        var jwt = scope.ServiceProvider.GetRequiredService<IJwtTokenService>();
        var token = jwt.GenerateToken(user);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        GC.Collect();
        GC.WaitForPendingFinalizers();
        foreach (var file in Directory.GetFiles(".", $"{_dbName}*"))
        {
            try { File.Delete(file); } catch { /* best effort */ }
        }
    }
}
