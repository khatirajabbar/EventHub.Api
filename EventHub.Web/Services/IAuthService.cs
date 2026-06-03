using EventHub.Web.Models.Auth;
using EventHub.Web.Models.DTOs;

namespace EventHub.Web.Services;

public interface IAuthService
{
    Task<AuthResponseDto> LoginAsync(LoginViewModel model);
    Task<RegisterResponseDto> RegisterAsync(RegisterViewModel model);
}
