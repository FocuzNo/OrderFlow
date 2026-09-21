using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderFlow.Inventory.Domain.Reservations;

namespace OrderFlow.Inventory.Infrastructure.Persistence.Configurations;

public sealed class StockReservationConfiguration : IEntityTypeConfiguration<StockReservation>
{
    public void Configure(EntityTypeBuilder<StockReservation> builder)
    {
        builder.ToTable("stock_reservations");
        builder.HasKey(x => x.Id);
        builder
            .Property(x => x.Status)
            .HasConversion(x => x.Value, x => ReservationStatus.FromValue(x));
        builder.HasIndex(x => x.OrderId);
    }
}
