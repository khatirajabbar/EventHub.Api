namespace EventHub.Api.DTOs.Event;

public class EventResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string? Description { get; set; }
    public DateTime Date { get; set; }
    public string Location { get; set; }
    public string? BannerImageUrl { get; set; }
    public int OrganizerId { get; set; }
}