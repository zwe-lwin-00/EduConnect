using EduConnect.API.Models;
using EduConnect.Shared.Extensions;
using EduConnect.Application.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text.Json;

namespace EduConnect.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IWebHostEnvironment _env;
    private readonly JsonSerializerOptions _jsonOptions;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IWebHostEnvironment env,
        IOptions<JsonOptions> jsonOptions)
    {
        _next = next;
        _logger = logger;
        _env = env;
        _jsonOptions = jsonOptions.Value.JsonSerializerOptions;
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
        var requestId = context.TraceIdentifier;
        var code = HttpStatusCode.InternalServerError;
        ApiErrorResponse response;

        switch (exception)
        {
            case NotFoundException notFoundException:
                code = HttpStatusCode.NotFound;
                response = new ApiErrorResponse { Error = notFoundException.Message, Code = notFoundException.Code, RequestId = requestId };
                break;
            case BusinessException businessException:
                code = HttpStatusCode.BadRequest;
                response = new ApiErrorResponse { Error = businessException.Message, Code = businessException.Code, RequestId = requestId };
                break;
            case UnauthorizedAccessException:
                code = HttpStatusCode.Unauthorized;
                response = new ApiErrorResponse { Error = "Unauthorized access", Code = "UNAUTHORIZED", RequestId = requestId };
                break;
            default:
                var message = "An error occurred while processing your request";
                if (_env.IsDevelopment() && exception is DbUpdateException dbEx)
                    message = dbEx.InnerException?.Message ?? dbEx.Message;
                response = new ApiErrorResponse { Error = message, Code = "INTERNAL_ERROR", RequestId = requestId };
                if (_env.IsDevelopment())
                    response.Details = new { exception = exception.Message, stackTrace = exception.StackTrace };
                break;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)code;
        var result = JsonSerializer.Serialize(response, _jsonOptions);
        return context.Response.WriteAsync(result);
    }
}
