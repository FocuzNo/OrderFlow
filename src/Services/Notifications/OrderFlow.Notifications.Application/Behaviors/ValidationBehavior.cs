namespace OrderFlow.Notifications.Application.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators
) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken
    )
    {
        var context = new ValidationContext<TRequest>(request);
        var results = await Task.WhenAll(
            validators.Select(candidate => candidate.ValidateAsync(context, cancellationToken))
        );
        var failures = results
            .SelectMany(candidate => candidate.Errors)
            .Where(candidate => candidate is not null)
            .ToArray();
        if (failures.Length > 0)
            throw new ValidationException(failures);
        return await next();
    }
}
