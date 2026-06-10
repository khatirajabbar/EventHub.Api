using EventHub.Api.Data;
using EventHub.Api.DTOs.Auth;
using EventHub.Api.DTOs.User;
using EventHub.Api.Models;
using EventHub.Api.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventHub.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public AuthController(IAuthService authService, AppDbContext context, IMapper mapper)
    {
        _authService = authService;
        _context = context;
        _mapper = mapper;
    }

    // POST /api/auth/register
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var result = await _authService.RegisterAsync(dto);
        return Ok(ApiResponse<RegisterResponseDto>.Ok(result, "Registration successful."));
    }

    // POST /api/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var result = await _authService.LoginAsync(dto);
        return Ok(ApiResponse<AuthResponseDto>.Ok(result, "Login successful."));
    }

    // POST /api/auth/refresh
    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto dto)
    {
        var result = await _authService.RefreshTokenAsync(dto.RefreshToken);
        return Ok(ApiResponse<AuthResponseDto>.Ok(result, "Token refreshed."));
    }

    // POST /api/auth/change-password
    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        var userIdClaim = User.FindFirst("sub")?.Value;
        if (userIdClaim == null || !int.TryParse(userIdClaim, out var userId))
            return Unauthorized(ApiResponse.Fail("Invalid token."));

        await _authService.ChangePasswordAsync(userId, dto.OldPassword, dto.NewPassword);
        return Ok(ApiResponse.OkNoData("Password changed successfully."));
    }

    // POST /api/auth/change-email
    [HttpPost("change-email")]
    [Authorize]
    public async Task<IActionResult> ChangeEmail([FromBody] ChangeEmailDto dto)
    {
        var userIdClaim = User.FindFirst("sub")?.Value;
        if (userIdClaim == null || !int.TryParse(userIdClaim, out var userId))
            return Unauthorized(ApiResponse.Fail("Invalid token."));

        await _authService.ChangeEmailAsync(userId, dto.NewEmail, dto.Password);
        return Ok(ApiResponse.OkNoData("Email changed successfully."));
    }

    // GET /api/auth/confirm-email
    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromQuery] string token, [FromQuery] string email)
    {
        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            return BadRequest(ApiResponse.Fail("Token and email are required."));

        await _authService.ConfirmEmailAsync(token, email);
        return Ok(ApiResponse.OkNoData("Email confirmed successfully! You can now login."));
    }

    // GET /api/auth/profile
    [HttpGet("profile")]
    [Authorize]
    public async Task<IActionResult> GetProfile()
    {
        var userIdClaim = User.FindFirst("sub")?.Value;
        if (userIdClaim == null)
            return Unauthorized(ApiResponse.Fail("Invalid token."));

        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized(ApiResponse.Fail("Invalid token format."));

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return NotFound(ApiResponse.Fail("User not found."));

        var profileDto = _mapper.Map<UserResponseDto>(user);
        return Ok(ApiResponse<UserResponseDto>.Ok(profileDto));
    }

    // POST /api/auth/forgot-password
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        await _authService.ForgotPasswordAsync(dto.Email);
        return Ok(ApiResponse.OkNoData("Password reset link has been sent to your email."));
    }

    // POST /api/auth/reset-password
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        if (string.IsNullOrEmpty(dto.Token) || string.IsNullOrEmpty(dto.Email))
            return BadRequest(ApiResponse.Fail("Token and email are required."));

        await _authService.ResetPasswordAsync(dto.Token, dto.Email, dto.NewPassword);
        return Ok(ApiResponse.OkNoData("Password reset successfully. You can now login."));
    }
}