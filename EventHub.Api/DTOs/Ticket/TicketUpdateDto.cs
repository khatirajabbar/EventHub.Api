namespace EventHub.Api.DTOs.Ticket;

public class TicketUpdateDto
{
    public int EventId { get; set; }
    public string Type { get; set; }
    public decimal Price { get; set; }
    public int QuantityAvailable { get; set; }
}