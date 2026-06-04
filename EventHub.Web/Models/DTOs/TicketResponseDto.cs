namespace EventHub.Web.Models.DTOs;

public class TicketResponseDto
{
    public int Id { get; set; }
    public string TicketType { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public int AvailableQuantity { get; set; }
    public int EventId { get; set; }
}