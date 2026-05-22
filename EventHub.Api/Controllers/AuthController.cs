using EventHub.Api.Data;
using EventHub.Api.DTOs.Auth;
using EventHub.Api.DTOs.User;
using EventHub.Api.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        try
        {
            var result = await _authService.RegisterAsync(dto);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // POST /api/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        try
        {
            var result = await _authService.LoginAsync(dto);
            return Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { message = "Invalid username or password." });
        }
    }

    // GET /api/auth/profile
    [HttpGet("profile")]
    [Authorize]
    public async Task<IActionResult> GetProfile()
    {
        try
        {
            // Get user ID from JWT token claims
            var userIdClaim = User.FindFirst("sub")?.Value;
            if (userIdClaim == null)
                return Unauthorized(new { message = "Invalid token." });

            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized(new { message = "Invalid token format." });

            // Fetch user from database
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound(new { message = "User not found." });

            // Map to response DTO
            var profileDto = _mapper.Map<UserResponseDto>(user);
            return Ok(profileDto);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving profile.", error = ex.Message });
        }
    }
}

