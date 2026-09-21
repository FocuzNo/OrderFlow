using OrderFlow.Ordering.Domain.Common;

namespace OrderFlow.Ordering.Domain.Orders;

public sealed record ShippingAddress
{
    private ShippingAddress() { }

    private ShippingAddress(string line1, string city, string postalCode, string country)
    {
        Line1 = line1;
        City = city;
        PostalCode = postalCode;
        Country = country;
    }

    public string Line1 { get; init; } = string.Empty;

    public string City { get; init; } = string.Empty;

    public string PostalCode { get; init; } = string.Empty;

    public string Country { get; init; } = string.Empty;

    public static ShippingAddress Create(
        string line1,
        string city,
        string postalCode,
        string country
    )
    {
        if (new[] { line1, city, postalCode, country }.Any(string.IsNullOrWhiteSpace))
            throw new DomainException("Complete shipping address is required.");
        return new ShippingAddress(line1.Trim(), city.Trim(), postalCode.Trim(), country.Trim());
    }
}
