using AutoMapper;
using EventHub.Api.Data;
using EventHub.Api.DTOs.Event;
using EventHub.Api.Entities;
using EventHub.Api.Models;
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

    private string GetAbsoluteUrl(string relativePath)
    {
        if (string.IsNullOrEmpty(relativePath)) return null;
        var request = HttpContext.Request;
        return $"{request.Scheme}://{request.Host}{relativePath}";
    }

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
    public async Task<IActionResult> GetAll()
    {
        var events = await _context.Events.Include(e => e.Organizer).ToListAsync();
        var result = _mapper.Map<List<EventResponseDto>>(events);
        result = result.Select(ApplyAbsoluteUrls).ToList();
        return Ok(ApiResponse<List<EventResponseDto>>.Ok(result));
    }

    // GET /api/events/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var ev = await _context.Events.Include(e => e.Organizer).FirstOrDefaultAsync(e => e.Id == id);
        if (ev == null)
            return NotFound(ApiResponse.Fail("Event not found."));
        var result = _mapper.Map<EventResponseDto>(ev);
        result = ApplyAbsoluteUrls(result);
        return Ok(ApiResponse<EventResponseDto>.Ok(result));
    }

    // POST /api/events
    [HttpPost]
    // [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] EventCreateDto dto)
    {
        var organizerExists = await _context.Organizers.AnyAsync(o => o.Id == dto.OrganizerId);
        if (!organizerExists)
            return BadRequest(ApiResponse.Fail("Organizer with the specified ID does not exist."));

        var ev = _mapper.Map<Event>(dto);
        _context.Events.Add(ev);
        await _context.SaveChangesAsync();
        var result = _mapper.Map<EventResponseDto>(ev);
        result = ApplyAbsoluteUrls(result);
        return CreatedAtAction(nameof(GetById), new { id = ev.Id },
            ApiResponse<EventResponseDto>.Ok(result, "Event created successfully."));
    }

    // PUT /api/events/{id}
    [HttpPut("{id}")]
    // [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] EventUpdateDto dto)
    {
        var ev = await _context.Events.FindAsync(id);
        if (ev == null)
            return NotFound(ApiResponse.Fail("Event not found."));

        var organizerExists = await _context.Organizers.AnyAsync(o => o.Id == dto.OrganizerId);
        if (!organizerExists)
            return BadRequest(ApiResponse.Fail("Organizer with the specified ID does not exist."));

        _mapper.Map(dto, ev);
        await _context.SaveChangesAsync();
        var result = _mapper.Map<EventResponseDto>(ev);
        result = ApplyAbsoluteUrls(result);
        return Ok(ApiResponse<EventResponseDto>.Ok(result, "Event updated successfully."));
    }

    // DELETE /api/events/{id}
    [HttpDelete("{id}")]
    // [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var ev = await _context.Events.FindAsync(id);
        if (ev == null)
            return NotFound(ApiResponse.Fail("Event not found."));
        _context.Events.Remove(ev);
        await _context.SaveChangesAsync();
        return Ok(ApiResponse.OkNoData("Event deleted successfully."));
    }

    // POST /api/events/{id}/banner
    [HttpPost("{id}/banner")]
    // [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UploadBanner(int id, IFormFile file)
    {
        var ev = await _context.Events.FindAsync(id);
        if (ev == null)
            return NotFound(ApiResponse.Fail("Event not found."));

        var url = await _fileService.SaveFileAsync(file, "banners");
        ev.BannerImageUrl = url;
        await _context.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(new { bannerUrl = GetAbsoluteUrl(url) }, "Banner uploaded successfully."));
    }

    // GET /api/events/{eventId}/tickets
    [HttpGet("{eventId}/tickets")]
    // [Authorize]
    public async Task<IActionResult> GetTickets(int eventId)
    {
        var exists = await _context.Events.AnyAsync(e => e.Id == eventId);
        if (!exists)
            return NotFound(ApiResponse.Fail("Event not found."));

        var tickets = await _context.Tickets
            .Where(t => t.EventId == eventId)
            .Include(t => t.Event)
            .ToListAsync();
        var result = _mapper.Map<List<DTOs.Ticket.TicketResponseDto>>(tickets);
        return Ok(ApiResponse<List<DTOs.Ticket.TicketResponseDto>>.Ok(result));
    }

    // POST /api/events/{eventId}/tickets
    [HttpPost("{eventId}/tickets")]
    // [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateTicket(int eventId, [FromBody] DTOs.Ticket.TicketCreateDto dto)
    {
        var exists = await _context.Events.AnyAsync(e => e.Id == eventId);
        if (!exists)
            return NotFound(ApiResponse.Fail("Event not found."));

        var ticket = _mapper.Map<Ticket>(dto);
        ticket.EventId = eventId;
        _context.Tickets.Add(ticket);
        await _context.SaveChangesAsync();
        var result = _mapper.Map<DTOs.Ticket.TicketResponseDto>(ticket);
        return Ok(ApiResponse<DTOs.Ticket.TicketResponseDto>.Ok(result, "Ticket created successfully."));
    }

    // GET /api/events/{eventId}/organizer
    [HttpGet("{eventId}/organizer")]
    // [Authorize]
    public async Task<IActionResult> GetOrganizer(int eventId)
    {
        var ev = await _context.Events.Include(e => e.Organizer).FirstOrDefaultAsync(e => e.Id == eventId);
        if (ev == null)
            return NotFound(ApiResponse.Fail("Event not found."));
        if (ev.Organizer == null)
            return NotFound(ApiResponse.Fail("Organizer not found for this event."));

        var result = _mapper.Map<DTOs.Organizer.OrganizerResponseDto>(ev.Organizer);
        if (!string.IsNullOrEmpty(result.LogoUrl))
            result.LogoUrl = GetAbsoluteUrl(result.LogoUrl);
        return Ok(ApiResponse<DTOs.Organizer.OrganizerResponseDto>.Ok(result));
    }
}