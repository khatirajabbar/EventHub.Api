using EventHub.Api.Enums;

namespace EventHub.Api.DTOs.Ticket;

public class TicketCreateDto
{
    public int EventId { get; set; }
    public TicketType Type { get; set; }
    public decimal Price { get; set; }
    public int QuantityAvailable { get; set; }
}