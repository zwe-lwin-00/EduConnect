using EduConnect.Shared.Extensions;
using EduConnect.Application.Common.Exceptions;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text.Json;

namespace EduConnect.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IWebHostEnvironment _env;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IWebHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.ErrorLog(ex, "Unhandled exception");
            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var code = HttpStatusCode.InternalServerError;
        var result = string.Empty;

        switch (exception)
        {
            case NotFoundException notFoundException:
                code = HttpStatusCode.NotFound;
                result = JsonSerializer.Serialize(new { error = notFoundException.Message, code = notFoundException.Code });
                break;
            case BusinessException businessException:
                code = HttpStatusCode.BadRequest;
                result = JsonSerializer.Serialize(new { error = businessException.Message, code = businessException.Code });
                break;
            case UnauthorizedAccessException:
                code = HttpStatusCode.Unauthorized;
                result = JsonSerializer.Serialize(new { error = "Unauthorized access", code = "UNAUTHORIZED" });
                break;
            default:
                var message = "An error occurred while processing your request";
                if (_env.IsDevelopment() && exception is DbUpdateException dbEx)
                    message = dbEx.InnerException?.Message ?? dbEx.Message;
                result = JsonSerializer.Serialize(new { error = message, code = "INTERNAL_ERROR" });
                break;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)code;
        return context.Response.WriteAsync(result);
    }
}
