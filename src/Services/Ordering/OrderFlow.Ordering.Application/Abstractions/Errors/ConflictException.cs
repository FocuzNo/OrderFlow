namespace OrderFlow.Ordering.Application.Abstractions.Errors;
public sealed class ConflictException(string message) : Exception(message);
