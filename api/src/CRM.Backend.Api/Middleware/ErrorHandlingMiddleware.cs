using System.Net;
using CRM.Backend.Domain.Exceptions;
using FluentValidation;

namespace CRM.Backend.Api.Middleware;

public class ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger = logger;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);

            if (context.Response.StatusCode == (int)HttpStatusCode.Unauthorized && !context.Response.HasStarted)
            {
                _logger.LogWarning("Unauthorized access attempt to {Path}", context.Request.Path);
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new 
                { 
                    error = "Authentication required. Please provide a valid Bearer token.",
                    details = "Use POST /auth/token to obtain an authentication token."
                });
            }
            else if (context.Response.StatusCode == (int)HttpStatusCode.Forbidden && !context.Response.HasStarted)
            {
                _logger.LogWarning("Forbidden access attempt to {Path}", context.Request.Path);
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new 
                { 
                    error = "Access forbidden. You don't have permission to access this resource."
                });
            }
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning("Validation error: {Errors}", ex.Message);
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            context.Response.ContentType = "application/json";
            var errors = ex.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
            await context.Response.WriteAsJsonAsync(new { errors });
        }
        catch (DomainException ex)
        {
            _logger.LogWarning("Domain error: {Message}", ex.Message);
            context.Response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error");
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new { error = "An unexpected error occurred." });
        }
    }
}