using AutoMapper;
using EventHub.Api.Data;
using EventHub.Api.DTOs.Event;
using EventHub.Api.Entities;
using EventHub.Api.Services;
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

    // GET /api/events
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var events = await _context.Events.Include(e => e.Organizer).ToListAsync();
        var result = _mapper.Map<List<EventResponseDto>>(events);
        return Ok(result);
    }

    // GET /api/events/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var ev = await _context.Events.Include(e => e.Organizer).FirstOrDefaultAsync(e => e.Id == id);
        if (ev == null) return NotFound();
        return Ok(_mapper.Map<EventResponseDto>(ev));
    }

    // POST /api/events
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] EventCreateDto dto)
    {
        var ev = _mapper.Map<Event>(dto);
        _context.Events.Add(ev);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = ev.Id }, _mapper.Map<EventResponseDto>(ev));
    }

    // PUT /api/events/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] EventUpdateDto dto)
    {
        var ev = await _context.Events.FindAsync(id);
        if (ev == null) return NotFound();
        _mapper.Map(dto, ev);
        await _context.SaveChangesAsync();
        return Ok(_mapper.Map<EventResponseDto>(ev));
    }

    // DELETE /api/events/{id}
    [HttpDelete("{id}")]
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
    public async Task<IActionResult> UploadBanner(int id, IFormFile file)
    {
        var ev = await _context.Events.FindAsync(id);
        if (ev == null) return NotFound();
        var url = await _fileService.SaveFileAsync(file, "banners");
        ev.BannerImageUrl = url;
        await _context.SaveChangesAsync();
        return Ok(new { bannerUrl = url });
    }

    // GET /api/events/{eventId}/tickets
    [HttpGet("{eventId}/tickets")]
    public async Task<IActionResult> GetTickets(int eventId)
    {
        var exists = await _context.Events.AnyAsync(e => e.Id == eventId);
        if (!exists) return NotFound();
        var tickets = await _context.Tickets.Where(t => t.EventId == eventId).ToListAsync();
        return Ok(tickets);
    }

    // POST /api/events/{eventId}/tickets
    [HttpPost("{eventId}/tickets")]
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
    public async Task<IActionResult> GetOrganizer(int eventId)
    {
        var ev = await _context.Events.Include(e => e.Organizer).FirstOrDefaultAsync(e => e.Id == eventId);
        if (ev == null) return NotFound();
        return Ok(_mapper.Map<DTOs.Organizer.OrganizerResponseDto>(ev.Organizer));
    }
}