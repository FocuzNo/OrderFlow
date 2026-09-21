namespace OrderFlow.Ordering.Application.Abstractions.Errors;

public sealed class NotFoundException(string message) : Exception(message);
