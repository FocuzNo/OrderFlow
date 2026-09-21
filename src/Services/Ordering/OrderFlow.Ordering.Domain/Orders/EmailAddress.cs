using System.Net.Mail;
using OrderFlow.Ordering.Domain.Common;

namespace OrderFlow.Ordering.Domain.Orders;

public readonly record struct EmailAddress
{
    public string Value { get; }

    private EmailAddress(string value) => Value = value;

    public static EmailAddress Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > 320)
            throw new DomainException("Customer email is invalid.");
        try
        {
            var address = new MailAddress(value.Trim());
            return new EmailAddress(address.Address);
        }
        catch (FormatException)
        {
            throw new DomainException("Customer email is invalid.");
        }
    }
}
