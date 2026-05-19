using AutoMapper;
using EventHub.Api.Data;
using EventHub.Api.DTOs.Organizer;
using EventHub.Api.Entities;
using EventHub.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Api.Controllers;

[ApiController]
[Route("api/organizers")]
public class OrganizersController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly FileService _fileService;

    public OrganizersController(AppDbContext context, IMapper mapper, FileService fileService)
    {
        _context = context;
        _mapper = mapper;
        _fileService = fileService;
    }

    // GET /api/organizers
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var organizers = await _context.Organizers.ToListAsync();
        return Ok(_mapper.Map<List<OrganizerResponseDto>>(organizers));
    }

    // GET /api/organizers/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var organizer = await _context.Organizers.FindAsync(id);
        if (organizer == null) return NotFound();
        return Ok(_mapper.Map<OrganizerResponseDto>(organizer));
    }

    // POST /api/organizers
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] OrganizerCreateDto dto)
    {
        var organizer = _mapper.Map<Organizer>(dto);
        _context.Organizers.Add(organizer);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = organizer.Id }, _mapper.Map<OrganizerResponseDto>(organizer));
    }

    // PUT /api/organizers/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] OrganizerUpdateDto dto)
    {
        var organizer = await _context.Organizers.FindAsync(id);
        if (organizer == null) return NotFound();
        _mapper.Map(dto, organizer);
        await _context.SaveChangesAsync();
        return Ok(_mapper.Map<OrganizerResponseDto>(organizer));
    }

    // DELETE /api/organizers/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var organizer = await _context.Organizers.FindAsync(id);
        if (organizer == null) return NotFound();
        _context.Organizers.Remove(organizer);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // POST /api/organizers/{id}/logo
    [HttpPost("{id}/logo")]
    public async Task<IActionResult> UploadLogo(int id, IFormFile file)
    {
        var organizer = await _context.Organizers.FindAsync(id);
        if (organizer == null) return NotFound();
        var url = await _fileService.SaveFileAsync(file, "logos");
        organizer.LogoUrl = url;
        await _context.SaveChangesAsync();
        return Ok(new { logoUrl = url });
    }

    // GET /api/organizers/{organizerId}/events
    [HttpGet("{organizerId}/events")]
    public async Task<IActionResult> GetEvents(int organizerId)
    {
        var exists = await _context.Organizers.AnyAsync(o => o.Id == organizerId);
        if (!exists) return NotFound();
        var events = await _context.Events
            .Where(e => e.OrganizerId == organizerId)
            .ToListAsync();
        return Ok(_mapper.Map<List<DTOs.Event.EventResponseDto>>(events));
    }
}