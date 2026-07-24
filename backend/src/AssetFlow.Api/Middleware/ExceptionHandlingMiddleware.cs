using System.Diagnostics;
using AssetFlow.Application.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;
using FluentValidationException = FluentValidation.ValidationException;

namespace AssetFlow.Api.Middleware;

/// <summary>
/// Converts unhandled exceptions into RFC 9457 <c>application/problem+json</c>
/// responses so the API never leaks a stack trace and every error has a
/// consistent, machine-readable shape. Domain exceptions map to intentful status
/// codes; anything else is a logged 500.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await WriteProblemAsync(context, exception);
        }
    }

    private async Task WriteProblemAsync(HttpContext context, Exception exception)
    {
        if (context.Response.HasStarted)
        {
            // The response is already on the wire; we can only log and bail out.
            _logger.LogError(exception, "Exception after response started for {Path}", context.Request.Path);
            return;
        }

        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;
        ProblemDetails problem;

        switch (exception)
        {
            case FluentValidationException validation:
                var errors = validation.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        group => group.Key,
                        group => group.Select(e => e.ErrorMessage).Distinct().ToArray());

                problem = new ValidationProblemDetails(errors)
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "One or more validation errors occurred.",
                    Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1"
                };
                _logger.LogWarning(exception, "Validation failed for {Path}", context.Request.Path);
                break;

            case NotFoundException:
                problem = CreateProblem(StatusCodes.Status404NotFound, "Resource not found.", exception.Message);
                _logger.LogWarning("Not found on {Path}: {Message}", context.Request.Path, exception.Message);
                break;

            case ConflictException:
                problem = CreateProblem(StatusCodes.Status409Conflict, "Request conflicts with the current state.", exception.Message);
                _logger.LogWarning("Conflict on {Path}: {Message}", context.Request.Path, exception.Message);
                break;

            case UnauthorizedException:
                problem = CreateProblem(StatusCodes.Status401Unauthorized, "Authentication failed.", exception.Message);
                _logger.LogWarning("Unauthorized on {Path}: {Message}", context.Request.Path, exception.Message);
                break;

            default:
                var detail = _environment.IsDevelopment()
                    ? exception.ToString()
                    : "An unexpected error occurred. Please try again later.";
                problem = CreateProblem(StatusCodes.Status500InternalServerError, "Internal server error.", detail);
                _logger.LogError(exception, "Unhandled exception on {Path}", context.Request.Path);
                break;
        }

        problem.Instance = context.Request.Path.Value;
        problem.Extensions["traceId"] = traceId;

        context.Response.Clear();
        context.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(problem, problem.GetType(), options: null, contentType: "application/problem+json");
    }

    private static ProblemDetails CreateProblem(int status, string title, string detail) => new()
    {
        Status = status,
        Title = title,
        Detail = detail
    };
}
