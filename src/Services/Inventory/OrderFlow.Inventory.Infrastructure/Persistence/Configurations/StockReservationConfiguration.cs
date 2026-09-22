using OrderFlow.Inventory.Domain.Reservations;

namespace OrderFlow.Inventory.Infrastructure.Persistence.Configurations;

public sealed class StockReservationConfiguration : IEntityTypeConfiguration<StockReservation>
{
    public void Configure(EntityTypeBuilder<StockReservation> builder)
    {
        builder.ToTable("stock_reservations");
        builder.HasKey(candidate => candidate.Id);
        builder
            .Property(candidate => candidate.Status)
            .HasConversion(
                candidate => candidate.Value,
                candidate => ReservationStatus.FromValue(candidate)
            );
        builder.HasIndex(candidate => candidate.OrderId);
    }
}
