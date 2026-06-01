using EventHub.Web.Models.DTOs;

namespace EventHub.Web.Services;

public class EventService : IEventService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<EventService> _logger;

    public EventService(IHttpClientFactory httpClientFactory, ILogger<EventService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("EventHubApi");
        _logger = logger;
    }

    public async Task<List<EventResponseDto>> GetAllEventsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/events");
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Failed to fetch events. Status code: {response.StatusCode}");
                return new List<EventResponseDto>();
            }

            var json = await response.Content.ReadAsStringAsync();
            var events = System.Text.Json.JsonSerializer.Deserialize<List<EventResponseDto>>(json, 
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            return events ?? new List<EventResponseDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching events: {ex.Message}");
            return new List<EventResponseDto>();
        }
    }

    public async Task<EventResponseDto> GetEventByIdAsync(int id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/events/{id}");
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Failed to fetch event {id}. Status code: {response.StatusCode}");
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            var @event = System.Text.Json.JsonSerializer.Deserialize<EventResponseDto>(json,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            return @event;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching event {id}: {ex.Message}");
            return null;
        }
    }

    public async Task<EventResponseDto> CreateEventAsync(EventCreateDto eventDto)
    {
        try
        {
            var json = System.Text.Json.JsonSerializer.Serialize(eventDto);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync("/api/events", content);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Failed to create event. Status code: {response.StatusCode}");
                return null;
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            var createdEvent = System.Text.Json.JsonSerializer.Deserialize<EventResponseDto>(responseJson,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            return createdEvent;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error creating event: {ex.Message}");
            return null;
        }
    }

    public async Task<EventResponseDto> UpdateEventAsync(int id, EventUpdateDto eventDto)
    {
        try
        {
            var json = System.Text.Json.JsonSerializer.Serialize(eventDto);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PutAsync($"/api/events/{id}", content);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Failed to update event {id}. Status code: {response.StatusCode}");
                return null;
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            var updatedEvent = System.Text.Json.JsonSerializer.Deserialize<EventResponseDto>(responseJson,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            return updatedEvent;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error updating event {id}: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> DeleteEventAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"/api/events/{id}");
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Failed to delete event {id}. Status code: {response.StatusCode}");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error deleting event {id}: {ex.Message}");
            return false;
        }
    }
}

