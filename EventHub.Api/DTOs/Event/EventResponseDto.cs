namespace EventHub.Api.DTOs.Event;
using EventHub.Api.DTOs.Organizer;

public class EventResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string? Description { get; set; }
    public DateTime Date { get; set; }
    public string Location { get; set; }
    public string? BannerImageUrl { get; set; }
    public int OrganizerId { get; set; }
    public OrganizerResponseDto Organizer { get; set; } 
}