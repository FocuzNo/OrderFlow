namespace OrderFlow.Notifications.Application.Abstractions.Errors;
public sealed class ConflictException(string message) : Exception(message);
