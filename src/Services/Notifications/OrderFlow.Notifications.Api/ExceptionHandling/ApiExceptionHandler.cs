using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using OrderFlow.Notifications.Application.Abstractions.Errors;
using OrderFlow.Notifications.Domain.Common;

namespace OrderFlow.Notifications.Api.ExceptionHandling;

public sealed class ApiExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        if (exception is ValidationException v)
        {
            await Results
                .ValidationProblem(
                    v.Errors.GroupBy(candidate => candidate.PropertyName)
                        .ToDictionary(
                            candidate => candidate.Key,
                            candidate => candidate.Select(failure => failure.ErrorMessage).ToArray()
                        )
                )
                .ExecuteAsync(httpContext);
            return true;
        }
        var status = exception switch
        {
            NotFoundException => 404,
            ConflictException => 409,
            DomainException => 400,
            _ => 500,
        };
        if (status == 500)
            return false;
        await Results
            .Problem(
                statusCode: status,
                title: status == 404 ? "Resource not found"
                    : status == 409 ? "Conflict"
                    : "Business rule violation",
                detail: exception.Message
            )
            .ExecuteAsync(httpContext);
        return true;
    }
}
