using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using TaskManager.Application.DTOs.Projects;
using TaskManager.Application.DTOs.Tasks;
using TaskManager.Application.DTOs.Users;
using TaskManager.Domain.Enums;

namespace TaskManager.Api.Tests.Infrastructure;

public abstract class TestBase : IClassFixture<ApiFactory>
{
    /// <summary>Default password for <see cref="CreateUserAsync"/> bodies (meets MinLength(8)).</summary>
    protected const string DefaultUserPassword = "TestUserPw1!";

    protected static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    protected readonly HttpClient Client;
    protected readonly ApiFactory Factory;

    protected TestBase(ApiFactory factory)
    {
        Factory = factory;
        Client = factory.CreateAuthenticatedClient();
    }

    protected async Task<T> ReadAsync<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(content, JsonOptions)!;
    }

    protected async Task<UserDto> CreateUserAsync(string name = "Test User", string email = "")
    {
        email = string.IsNullOrEmpty(email) ? $"{Guid.NewGuid()}@test.com" : email;
        var response = await Client.PostAsJsonAsync("/users", new { name, email, password = DefaultUserPassword });
        response.EnsureSuccessStatusCode();
        return await ReadAsync<UserDto>(response);
    }

    protected async Task<ProjectDto> CreateProjectAsync(string name = "")
    {
        name = string.IsNullOrEmpty(name) ? $"Project-{Guid.NewGuid()}" : name;
        var response = await Client.PostAsJsonAsync("/projects", new { name });
        response.EnsureSuccessStatusCode();
        return await ReadAsync<ProjectDto>(response);
    }

    protected async Task<TaskDto> CreateTaskAsync(int projectId, string title = "Test Task", Priority priority = Priority.Medium)
    {
        var response = await Client.PostAsJsonAsync($"/projects/{projectId}/tasks",
            new { title, priority = priority.ToString() });
        response.EnsureSuccessStatusCode();
        return await ReadAsync<TaskDto>(response);
    }
}
