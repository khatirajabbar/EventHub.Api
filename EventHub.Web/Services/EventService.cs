using EventHub.Web.Models.DTOs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;


namespace EventHub.Web.Services;

public class EventService : IEventService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<EventService> _logger;

    public EventService(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor, ILogger<EventService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    // Creates an HttpClient and attaches the JWT token from the logged-in user's cookie claims
    private HttpClient CreateAuthorizedClient()
    {
        var client = _httpClientFactory.CreateClient("EventHubApi");

        var token = _httpContextAccessor.HttpContext?.User?.FindFirst("Token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        return client;
    }

    private static readonly System.Text.Json.JsonSerializerOptions JsonOptions =
        new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };

    public async Task<List<EventResponseDto>> GetAllEventsAsync()
    {
        try
        {
            var client = CreateAuthorizedClient();
            var response = await client.GetAsync("/api/events");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to fetch events. Status code: {StatusCode}", response.StatusCode);
                return new List<EventResponseDto>();
            }

            var json = await response.Content.ReadAsStringAsync();
            return System.Text.Json.JsonSerializer.Deserialize<List<EventResponseDto>>(json, JsonOptions)
                   ?? new List<EventResponseDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching events");
            return new List<EventResponseDto>();
        }
    }

    public async Task<EventResponseDto?> GetEventByIdAsync(int id)
    {
        try
        {
            var client = CreateAuthorizedClient();
            var response = await client.GetAsync($"/api/events/{id}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to fetch event {Id}. Status code: {StatusCode}", id, response.StatusCode);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            return System.Text.Json.JsonSerializer.Deserialize<EventResponseDto>(json, JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching event {Id}", id);
            return null;
        }
    }

    public async Task<EventResponseDto?> CreateEventAsync(EventCreateDto eventDto)
    {
        try
        {
            var client = CreateAuthorizedClient();
            var json = System.Text.Json.JsonSerializer.Serialize(eventDto);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/api/events", content);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to create event. Status code: {StatusCode}", response.StatusCode);
                return null;
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            return System.Text.Json.JsonSerializer.Deserialize<EventResponseDto>(responseJson, JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating event");
            return null;
        }
    }

    public async Task<EventResponseDto?> UpdateEventAsync(int id, EventUpdateDto eventDto)
    {
        try
        {
            var client = CreateAuthorizedClient();
            var json = System.Text.Json.JsonSerializer.Serialize(eventDto);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"/api/events/{id}", content);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to update event {Id}. Status code: {StatusCode}", id, response.StatusCode);
                return null;
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            return System.Text.Json.JsonSerializer.Deserialize<EventResponseDto>(responseJson, JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating event {Id}", id);
            return null;
        }
    }

    public async Task<bool> DeleteEventAsync(int id)
    {
        try
        {
            var client = CreateAuthorizedClient();
            var response = await client.DeleteAsync($"/api/events/{id}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to delete event {Id}. Status code: {StatusCode}", id, response.StatusCode);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting event {Id}", id);
            return false;
        }
    }

    public async Task<List<TicketResponseDto>> GetTicketsByEventIdAsync(int eventId)
    {
        try
        {
            var client = CreateAuthorizedClient();
            var response = await client.GetAsync($"/api/events/{eventId}/tickets");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to fetch tickets for event {EventId}. Status code: {StatusCode}", eventId, response.StatusCode);
                return new List<TicketResponseDto>();
            }

            var json = await response.Content.ReadAsStringAsync();
            return System.Text.Json.JsonSerializer.Deserialize<List<TicketResponseDto>>(json, JsonOptions)
                   ?? new List<TicketResponseDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching tickets for event {EventId}", eventId);
            return new List<TicketResponseDto>();
        }
    }
    public async Task<List<OrganizerResponseDto>> GetAllOrganizersAsync()
    {
        try
        {
            var client = CreateAuthorizedClient();
            var response = await client.GetAsync("/api/organizers");
            if (!response.IsSuccessStatusCode) return new List<OrganizerResponseDto>();
            var json = await response.Content.ReadAsStringAsync();
            return System.Text.Json.JsonSerializer.Deserialize<List<OrganizerResponseDto>>(json, JsonOptions)
                   ?? new List<OrganizerResponseDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching organizers");
            return new List<OrganizerResponseDto>();
        }
    }
}