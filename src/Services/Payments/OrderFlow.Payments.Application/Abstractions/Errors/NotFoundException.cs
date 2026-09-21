namespace OrderFlow.Payments.Application.Abstractions.Errors;

public sealed class NotFoundException(string message) : Exception(message);
