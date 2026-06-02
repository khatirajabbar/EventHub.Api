using EventHub.Web.Models.Auth;
using EventHub.Web.Models.DTOs;
using System.Text.Json;

namespace EventHub.Web.Services;

public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IHttpClientFactory httpClientFactory, ILogger<AuthService> logger)
    {
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

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Failed to login. Status: {response.StatusCode}, Error: {json}");
                throw new Exception("Invalid username or password.");
            }

            return JsonSerializer.Deserialize<AuthResponseDto>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error during login: {ex.Message}");
            throw;
        }
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterViewModel model)
    {
        try
        {
            var content = new StringContent(JsonSerializer.Serialize(model), System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("/api/auth/register", content);

            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Failed to register. Status: {response.StatusCode}, Error: {json}");
                throw new Exception("Registration failed. Please try again.");
            }

            return JsonSerializer.Deserialize<AuthResponseDto>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error during registration: {ex.Message}");
            throw;
        }
    }
}

