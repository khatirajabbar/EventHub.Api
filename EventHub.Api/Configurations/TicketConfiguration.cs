using EventHub.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventHub.Api.Configurations;

public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Type).IsRequired().HasMaxLength(50);
        builder.Property(t => t.Price).HasColumnType("decimal(18,2)");
        builder.HasOne(t => t.Event)
            .WithMany(e => e.Tickets)
            .HasForeignKey(t => t.EventId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}