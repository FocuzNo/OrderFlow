using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using OrderFlow.Catalog.Application.Abstractions.Errors;
using OrderFlow.Catalog.Domain.Common;

namespace OrderFlow.Catalog.Api.ExceptionHandling;

public sealed class ApiExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext c, Exception e, CancellationToken ct)
    {
        if (e is ValidationException v)
        {
            await Results
                .ValidationProblem(
                    v.Errors.GroupBy(x => x.PropertyName)
                        .ToDictionary(x => x.Key, x => x.Select(y => y.ErrorMessage).ToArray())
                )
                .ExecuteAsync(c);
            return true;
        }
        var status = e switch
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
                detail: e.Message
            )
            .ExecuteAsync(c);
        return true;
    }
}
