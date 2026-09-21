namespace OrderFlow.Payments.Application.Abstractions.Errors;

public sealed class ConflictException(string message) : Exception(message);
