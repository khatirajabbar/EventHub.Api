using AutoMapper;
using EventHub.Api.Data;
using EventHub.Api.DTOs.Event;
using EventHub.Api.Entities;
using EventHub.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Api.Controllers;

[ApiController]
[Route("api/events")]
public class EventsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly FileService _fileService;

    public EventsController(AppDbContext context, IMapper mapper, FileService fileService)
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

    // Helper method to apply absolute URLs to event DTO
    private EventResponseDto ApplyAbsoluteUrls(EventResponseDto eventDto)
    {
        if (eventDto != null)
        {
            if (!string.IsNullOrEmpty(eventDto.BannerImageUrl))
                eventDto.BannerImageUrl = GetAbsoluteUrl(eventDto.BannerImageUrl);
            
            if (eventDto.Organizer != null && !string.IsNullOrEmpty(eventDto.Organizer.LogoUrl))
                eventDto.Organizer.LogoUrl = GetAbsoluteUrl(eventDto.Organizer.LogoUrl);
        }
        return eventDto;
    }

    // GET /api/events
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll()
    {
        var events = await _context.Events.Include(e => e.Organizer).ToListAsync();
        var result = _mapper.Map<List<EventResponseDto>>(events);
        result = result.Select(ApplyAbsoluteUrls).ToList();
        return Ok(result);
    }

    // GET /api/events/{id}
    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetById(int id)
    {
        var ev = await _context.Events.Include(e => e.Organizer).FirstOrDefaultAsync(e => e.Id == id);
        if (ev == null) return NotFound();
        var result = _mapper.Map<EventResponseDto>(ev);
        result = ApplyAbsoluteUrls(result);
        return Ok(result);
    }

    // POST /api/events
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] EventCreateDto dto)
    {
        var organizerExists = await _context.Organizers.AnyAsync(o => o.Id == dto.OrganizerId);
        if (!organizerExists)
            return BadRequest(new { error = "Organizer with the specified ID does not exist." });

        var ev = _mapper.Map<Event>(dto);
        _context.Events.Add(ev);
        await _context.SaveChangesAsync();
        var result = _mapper.Map<EventResponseDto>(ev);
        result = ApplyAbsoluteUrls(result);
        return CreatedAtAction(nameof(GetById), new { id = ev.Id }, result);
    }

    // PUT /api/events/{id}
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] EventUpdateDto dto)
    {
        var ev = await _context.Events.FindAsync(id);
        if (ev == null) return NotFound();

        var organizerExists = await _context.Organizers.AnyAsync(o => o.Id == dto.OrganizerId);
        if (!organizerExists)
            return BadRequest(new { error = "Organizer with the specified ID does not exist." });

        _mapper.Map(dto, ev);
        await _context.SaveChangesAsync();
        var result = _mapper.Map<EventResponseDto>(ev);
        result = ApplyAbsoluteUrls(result);
        return Ok(result);
    }

    // DELETE /api/events/{id}
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var ev = await _context.Events.FindAsync(id);
        if (ev == null) return NotFound();
        _context.Events.Remove(ev);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // POST /api/events/{id}/banner
    [HttpPost("{id}/banner")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UploadBanner(int id, IFormFile file)
    {
        try
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev == null) return NotFound();
            var url = await _fileService.SaveFileAsync(file, "banners");
            ev.BannerImageUrl = url;
            await _context.SaveChangesAsync();
            return Ok(new { bannerUrl = GetAbsoluteUrl(url) });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // GET /api/events/{eventId}/tickets
    [HttpGet("{eventId}/tickets")]
    [Authorize]
    public async Task<IActionResult> GetTickets(int eventId)
    {
        var exists = await _context.Events.AnyAsync(e => e.Id == eventId);
        if (!exists) return NotFound();
        var tickets = await _context.Tickets
            .Where(t => t.EventId == eventId)
            .Include(t => t.Event)
            .ToListAsync();
        return Ok(_mapper.Map<List<DTOs.Ticket.TicketResponseDto>>(tickets));
    }

    // POST /api/events/{eventId}/tickets
    [HttpPost("{eventId}/tickets")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateTicket(int eventId, [FromBody] DTOs.Ticket.TicketCreateDto dto)
    {
        var exists = await _context.Events.AnyAsync(e => e.Id == eventId);
        if (!exists) return NotFound();
        var ticket = _mapper.Map<Ticket>(dto);
        ticket.EventId = eventId;
        _context.Tickets.Add(ticket);
        await _context.SaveChangesAsync();
        return Ok(_mapper.Map<DTOs.Ticket.TicketResponseDto>(ticket));
    }

    // GET /api/events/{eventId}/organizer
    [HttpGet("{eventId}/organizer")]
    [Authorize]
    public async Task<IActionResult> GetOrganizer(int eventId)
    {
        var ev = await _context.Events.Include(e => e.Organizer).FirstOrDefaultAsync(e => e.Id == eventId);
        if (ev == null) return NotFound();
        if (ev.Organizer == null) return NotFound(new { error = "Organizer not found for this event." });
        var result = _mapper.Map<DTOs.Organizer.OrganizerResponseDto>(ev.Organizer);
        if (!string.IsNullOrEmpty(result.LogoUrl))
            result.LogoUrl = GetAbsoluteUrl(result.LogoUrl);
        return Ok(result);
    }
}