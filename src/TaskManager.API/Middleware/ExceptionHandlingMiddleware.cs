using System.Text.Json;
using TaskManager.Domain.Exceptions;
using ValidationException = TaskManager.Domain.Exceptions.ValidationException;

namespace TaskManager.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, type, message, errors) = exception switch
        {
            NotFoundException nfe => (StatusCodes.Status404NotFound, "not_found", nfe.Message, (object)new { }),
            ConflictException ce => (StatusCodes.Status409Conflict, "conflict", ce.Message, (object)new { }),
            ForbiddenException fe => (StatusCodes.Status403Forbidden, "forbidden", fe.Message, (object)new { }),
            UnauthorizedException ue => (StatusCodes.Status401Unauthorized, "unauthorized", ue.Message, (object)new { }),
            ValidationException ve => (StatusCodes.Status400BadRequest, "validation_error", ve.Message, (object)ve.Errors),
            _ => (StatusCodes.Status500InternalServerError, "unexpected_error", "An unexpected error occurred.", (object)new { })
        };

        if (statusCode == 500)
            _logger.LogError(exception, "Unhandled exception");

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var response = new { type, message, errors };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
    }
}
