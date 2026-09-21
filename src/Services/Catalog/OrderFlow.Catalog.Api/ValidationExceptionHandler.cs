using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;

namespace OrderFlow.Catalog.Api;

public sealed class ValidationExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is KeyNotFoundException)
        {
            httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            await Results.Problem(statusCode: 404, title: "Resource not found", detail: exception.Message)
                .ExecuteAsync(httpContext);
            return true;
        }

        if (exception is ArgumentException or InvalidOperationException)
        {
            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await Results.Problem(statusCode: 400, title: "Business rule violation", detail: exception.Message)
                .ExecuteAsync(httpContext);
            return true;
        }

        if (exception is not ValidationException validationException)
        {
            return false;
        }

        var errors = validationException.Errors
            .GroupBy(error => error.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group.Select(error => error.ErrorMessage).ToArray());

        await Results.ValidationProblem(errors).ExecuteAsync(httpContext);

        return true;
    }
}
