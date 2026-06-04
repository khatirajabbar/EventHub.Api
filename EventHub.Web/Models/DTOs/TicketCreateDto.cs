namespace EventHub.Web.Models.DTOs;

public class TicketCreateDto
{
    public string Type { get; set; }
    public decimal Price { get; set; }
    public int QuantityAvailable { get; set; }
}