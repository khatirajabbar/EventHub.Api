using AutoMapper;
using EventHub.Api.Data;
using EventHub.Api.DTOs.Organizer;
using EventHub.Api.Entities;
using EventHub.Api.Services;
using Microsoft.AspNetCore.Authorization;
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

    // Helper method to convert relative URLs to absolute URLs
    private string GetAbsoluteUrl(string relativePath)
    {
        if (string.IsNullOrEmpty(relativePath))
            return null;
        
        var request = HttpContext.Request;
        var baseUrl = $"{request.Scheme}://{request.Host}";
        return $"{baseUrl}{relativePath}";
    }

    // Helper method to apply absolute URLs to organizer DTO
    private OrganizerResponseDto ApplyAbsoluteUrls(OrganizerResponseDto organizer)
    {
        if (organizer != null && !string.IsNullOrEmpty(organizer.LogoUrl))
        {
            organizer.LogoUrl = GetAbsoluteUrl(organizer.LogoUrl);
        }
        return organizer;
    }

    // GET /api/organizers
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll()
    {
        var organizers = await _context.Organizers.ToListAsync();
        var result = _mapper.Map<List<OrganizerResponseDto>>(organizers);
        result = result.Select(ApplyAbsoluteUrls).ToList();
        return Ok(result);
    }

    // GET /api/organizers/{id}
    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetById(int id)
    {
        var organizer = await _context.Organizers.FindAsync(id);
        if (organizer == null) return NotFound();
        var result = _mapper.Map<OrganizerResponseDto>(organizer);
        result = ApplyAbsoluteUrls(result);
        return Ok(result);
    }

    // POST /api/organizers
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] OrganizerCreateDto dto)
    {
        var organizer = _mapper.Map<Organizer>(dto);
        _context.Organizers.Add(organizer);
        await _context.SaveChangesAsync();
        var result = _mapper.Map<OrganizerResponseDto>(organizer);
        result = ApplyAbsoluteUrls(result);
        return CreatedAtAction(nameof(GetById), new { id = organizer.Id }, result);
    }

    // PUT /api/organizers/{id}
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] OrganizerUpdateDto dto)
    {
        var organizer = await _context.Organizers.FindAsync(id);
        if (organizer == null) return NotFound();
        _mapper.Map(dto, organizer);
        await _context.SaveChangesAsync();
        var result = _mapper.Map<OrganizerResponseDto>(organizer);
        result = ApplyAbsoluteUrls(result);
        return Ok(result);
    }

    // DELETE /api/organizers/{id}
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
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
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UploadLogo(int id, IFormFile file)
    {
        try
        {
            var organizer = await _context.Organizers.FindAsync(id);
            if (organizer == null) return NotFound();
            var url = await _fileService.SaveFileAsync(file, "logos");
            organizer.LogoUrl = url;
            await _context.SaveChangesAsync();
            return Ok(new { logoUrl = GetAbsoluteUrl(url) });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // GET /api/organizers/{organizerId}/events
    [HttpGet("{organizerId}/events")]
    [Authorize]
    public async Task<IActionResult> GetEvents(int organizerId)
    {
        var exists = await _context.Organizers.AnyAsync(o => o.Id == organizerId);
        if (!exists) return NotFound();
        var events = await _context.Events
            .Where(e => e.OrganizerId == organizerId)
            .Include(e => e.Organizer)
            .ToListAsync();
        var result = _mapper.Map<List<DTOs.Event.EventResponseDto>>(events);
        result = result.Select(e =>
        {
            if (e.Organizer != null && !string.IsNullOrEmpty(e.Organizer.LogoUrl))
                e.Organizer.LogoUrl = GetAbsoluteUrl(e.Organizer.LogoUrl);
            if (!string.IsNullOrEmpty(e.BannerImageUrl))
                e.BannerImageUrl = GetAbsoluteUrl(e.BannerImageUrl);
            return e;
        }).ToList();
        return Ok(result);
    }
}