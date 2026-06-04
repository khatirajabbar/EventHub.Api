namespace EventHub.Web.Models.DTOs;

public class TicketResponseDto
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public string Type { get; set; }       // comes as string e.g. "VIP", "General"
    public decimal Price { get; set; }
    public int QuantityAvailable { get; set; }
}