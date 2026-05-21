using AutoMapper;
using EventHub.Api.Data;
using EventHub.Api.DTOs.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public UsersController(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    // GET /api/users
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _context.Users.ToListAsync();
        return Ok(_mapper.Map<List<UserResponseDto>>(users));
    }

    // GET /api/users/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();
        return Ok(_mapper.Map<UserResponseDto>(user));
    }

    // PUT /api/users/{id}/role
    [HttpPut("{id}/role")]
    public async Task<IActionResult> UpdateRole(int id, [FromBody] UpdateUserRoleDto dto)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();

        // Prevent changing own role
        var currentUserId = User.FindFirst("sub")?.Value;
        if (currentUserId != null && int.Parse(currentUserId) == id)
            return BadRequest(new { error = "You cannot change your own role." });

        user.Role = dto.Role;
        await _context.SaveChangesAsync();
        return Ok(new { message = $"User role updated to {dto.Role}.", user = _mapper.Map<UserResponseDto>(user) });
    }
}

