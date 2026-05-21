using EventHub.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventHub.Api.Configurations;

public class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Title).IsRequired().HasMaxLength(150);
        builder.Property(e => e.Description).HasMaxLength(500);
        builder.Property(e => e.Location).IsRequired().HasMaxLength(200);
        builder.HasOne(e => e.Organizer)
            .WithMany(o => o.Events)
            .HasForeignKey(e => e.OrganizerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}