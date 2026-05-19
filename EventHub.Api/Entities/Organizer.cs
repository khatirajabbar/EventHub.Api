namespace EventHub.Api.Entities;
public class Organizer
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string? Phone { get; set; }
    public string? LogoUrl { get; set; }
    public ICollection<Event> Events { get; set; }
}