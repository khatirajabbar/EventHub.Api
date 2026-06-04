using EventHub.Web.Models.Auth;
using EventHub.Web.Models.DTOs;
using System.Text.Json;

namespace EventHub.Web.Services;

public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AuthService> _logger;

    private static readonly JsonSerializerOptions JsonOptions =
        new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

    public AuthService(IHttpClientFactory httpClientFactory, ILogger<AuthService> logger)    {
        _httpClient = httpClientFactory.CreateClient("EventHubApi");
        _logger = logger;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginViewModel model)
    {
        try
        {
            var content = new StringContent(JsonSerializer.Serialize(model), System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("/api/auth/login", content);
            var json = await response.Content.ReadAsStringAsync();

            // Always try to deserialize to our wrapper, as even errors use the ApiResponse format now
            var wrapped = JsonSerializer.Deserialize<ApiResponseWrapper<AuthResponseDto>>(json, JsonOptions);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to login. Status: {StatusCode}, Error: {Message}", response.StatusCode, wrapped?.Message);
                // Throw the actual message sent back by the API/Exception handler
                throw new Exception(wrapped?.Message ?? "Invalid username or password.");
            }

            return wrapped?.Data ?? throw new Exception("Invalid response data received from server.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during LoginAsync execution");
            throw;
        }
    }

    public async Task<RegisterResponseDto> RegisterAsync(RegisterViewModel model)
    {
        try
        {
            var content = new StringContent(JsonSerializer.Serialize(model), System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("/api/auth/register", content);
            var json = await response.Content.ReadAsStringAsync();

            // Always try to deserialize to our wrapper, as even errors use the ApiResponse format now
            var wrapped = JsonSerializer.Deserialize<ApiResponseWrapper<RegisterResponseDto>>(json, JsonOptions);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to register. Status: {StatusCode}, Error: {Message}", response.StatusCode, wrapped?.Message);
                // Throw the actual message sent back by the API/Exception handler
                throw new Exception(wrapped?.Message ?? "Registration failed. Please try again.");
            }

            return wrapped?.Data ?? throw new Exception("Invalid response data received from server.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during RegisterAsync execution");
            throw;
        }
    }
}