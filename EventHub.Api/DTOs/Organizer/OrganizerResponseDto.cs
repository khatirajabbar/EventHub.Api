namespace EventHub.Api.DTOs.Organizer;

public class OrganizerResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string? Phone { get; set; }
    public string? LogoUrl { get; set; }
}