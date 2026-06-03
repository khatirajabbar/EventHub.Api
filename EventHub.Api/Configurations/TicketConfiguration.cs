using EventHub.Api.Entities;
using EventHub.Api.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventHub.Api.Configurations;

public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Type)
            .IsRequired()
            .HasConversion(
                ticketType => ToDatabaseValue(ticketType),
                value => FromDatabaseValue(value))
            .HasMaxLength(50);
        builder.Property(t => t.Price);
        builder.HasOne(t => t.Event)
            .WithMany(e => e.Tickets)
            .HasForeignKey(t => t.EventId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static string ToDatabaseValue(TicketType ticketType)
    {
        return ticketType == TicketType.EarlyBird ? "Early Bird" : ticketType.ToString();
    }

    private static TicketType FromDatabaseValue(string value)
    {
        var normalized = value.Replace(" ", string.Empty);
        return Enum.TryParse<TicketType>(normalized, ignoreCase: true, out var ticketType)
            ? ticketType
            : TicketType.Unknown;
    }
}
