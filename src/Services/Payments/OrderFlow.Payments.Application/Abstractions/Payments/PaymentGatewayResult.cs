namespace OrderFlow.Payments.Application.Abstractions.Payments;
public sealed record PaymentGatewayResult(bool Succeeded,string? Reference,string? Error);
