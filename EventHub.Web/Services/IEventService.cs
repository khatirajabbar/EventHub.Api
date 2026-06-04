
using EventHub.Web.Models.DTOs;


namespace EventHub.Web.Services;

public interface IEventService
{
    Task<List<EventResponseDto>> GetAllEventsAsync();
    Task<List<OrganizerResponseDto>> GetAllOrganizersAsync();
    Task<EventResponseDto> GetEventByIdAsync(int id);
    Task<EventResponseDto> CreateEventAsync(EventCreateDto eventDto);
    Task<EventResponseDto> UpdateEventAsync(int id, EventUpdateDto eventDto);
    Task<bool> DeleteEventAsync(int id);
    Task<List<TicketResponseDto>> GetTicketsByEventIdAsync(int eventId);
    Task<OrganizerResponseDto?> CreateOrganizerAsync(OrganizerCreateDto dto);
    Task<TicketResponseDto?> CreateTicketAsync(int eventId, TicketCreateDto dto);
}