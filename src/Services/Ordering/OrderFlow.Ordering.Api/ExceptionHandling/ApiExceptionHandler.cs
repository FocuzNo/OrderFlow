using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using OrderFlow.Ordering.Application.Abstractions.Errors;
using OrderFlow.Ordering.Domain.Common;

namespace OrderFlow.Ordering.Api.ExceptionHandling;

public sealed class ApiExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        if (exception is ValidationException validationException)
        {
            await Results
                .ValidationProblem(
                    validationException
                        .Errors.GroupBy(candidate => candidate.PropertyName)
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
            DomainException => 409,
            Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException => 409,
            Microsoft.EntityFrameworkCore.DbUpdateException
            {
                InnerException: Npgsql.PostgresException { SqlState: "23505" }
            } => 409,
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
                detail: exception is Microsoft.EntityFrameworkCore.DbUpdateException
                    ? "Persistence conflict. Refresh the resource and retry."
                    : exception.Message
            )
            .ExecuteAsync(httpContext);
        return true;
    }
}
