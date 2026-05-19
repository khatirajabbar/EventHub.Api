namespace EventHub.Api.DTOs.Ticket;

public class TicketResponseDto
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public string Type { get; set; }
    public decimal Price { get; set; }
    public int QuantityAvailable { get; set; }
}