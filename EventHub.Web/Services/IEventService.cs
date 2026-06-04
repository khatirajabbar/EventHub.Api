
using EventHub.Web.Models.DTOs;


namespace EventHub.Web.Services;

public interface IEventService
{
    Task<List<EventResponseDto>> GetAllEventsAsync();
    Task<EventResponseDto> GetEventByIdAsync(int id);
    Task<EventResponseDto> CreateEventAsync(EventCreateDto eventDto);
    Task<EventResponseDto> UpdateEventAsync(int id, EventUpdateDto eventDto);
    Task<bool> DeleteEventAsync(int id);
    Task<List<TicketResponseDto>> GetTicketsByEventIdAsync(int eventId);
}