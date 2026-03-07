using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Treasara.Api.Dtos;
using Treasara.Domain;

namespace Treasara.Api.Exceptions;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, error, message, level) = exception switch
        {
            DomainValidationException => (
                StatusCodes.Status400BadRequest,
                "domain_validation_error",
                exception.Message,
                LogLevel.Warning),

            ArgumentException => (
                StatusCodes.Status400BadRequest,
                "validation_error",
                exception.Message,
                LogLevel.Warning),

            _ => (
                StatusCodes.Status500InternalServerError,
                "internal_server_error",
                "An unexpected error occurred.",
                LogLevel.Error)
        };

        _logger.Log(
            level,
            exception,
            "Unhandled exception for {Method} {Path}. TraceId: {TraceId}",
            httpContext.Request.Method,
            httpContext.Request.Path,
            httpContext.TraceIdentifier);

        if (httpContext.Response.HasStarted)
        {
            return false;
        }

        httpContext.Response.Clear();
        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/json";

        var dto = new ApiErrorDto
        {
            Error = error,
            Message = message,
            TraceId = httpContext.TraceIdentifier
        };

        await httpContext.Response.WriteAsync(
            JsonSerializer.Serialize(dto),
            cancellationToken);

        return true;
    }
}