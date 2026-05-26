using EventHub.Api.DTOs.Auth;

namespace EventHub.Api.Services;

public interface IAuthService
{
    Task<RegisterResponseDto> RegisterAsync(RegisterDto dto);
    Task<AuthResponseDto> LoginAsync(LoginDto dto);
    Task<AuthResponseDto> RefreshTokenAsync(string refreshToken);
    Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword);
    Task<bool> ChangeEmailAsync(int userId, string newEmail, string password);
    Task<bool> ConfirmEmailAsync(string token, string email);
    Task<bool> ForgotPasswordAsync(string email);
    Task<bool> ResetPasswordAsync(string token, string email, string newPassword);
}

