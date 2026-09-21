namespace OrderFlow.Inventory.Application.Abstractions.Errors;

public sealed class NotFoundException(string message) : Exception(message);
