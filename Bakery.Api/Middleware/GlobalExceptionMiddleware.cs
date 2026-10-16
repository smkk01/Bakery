using System.ComponentModel.DataAnnotations;
using Bakery.Application.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace Bakery.Api.Middleware;

public sealed class GlobalExceptionMiddleware(
    RequestDelegate next,
    ILogger<GlobalExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            logger.LogDebug("Request was cancelled by the client.");
        }
        catch (Exception exception)
        {
            if (context.Response.HasStarted)
            {
                logger.LogError(exception, "An exception occurred after the response started.");
                throw;
            }

            var isConflict = exception is EmailAlreadyRegisteredException;
            var isClientError = exception is ValidationException or ArgumentException;
            var statusCode = isConflict
                ? StatusCodes.Status409Conflict
                : isClientError
                    ? StatusCodes.Status400BadRequest
                    : StatusCodes.Status500InternalServerError;

            if (isClientError || isConflict)
            {
                logger.LogWarning(exception, "Request validation failed.");
            }
            else
            {
                logger.LogError(exception, "An unhandled exception occurred while processing the request.");
            }

            context.Response.Clear();
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/problem+json";

            var problem = new ProblemDetails
            {
                Status = statusCode,
                Title = isConflict
                    ? "Account already exists"
                    : isClientError
                        ? "Invalid request"
                        : "An unexpected error occurred.",
                Detail = isConflict || isClientError
                    ? exception.Message
                    : "The server could not complete the request.",
                Instance = context.Request.Path
            };
            problem.Extensions["traceId"] = context.TraceIdentifier;

            await context.Response.WriteAsJsonAsync(problem, cancellationToken: context.RequestAborted);
        }
    }
}
