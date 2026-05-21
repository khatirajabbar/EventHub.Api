using AutoMapper;
using EventHub.Api.Data;
using EventHub.Api.DTOs.Ticket;
using EventHub.Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Api.Controllers;

[ApiController]
[Route("api/tickets")]
public class TicketsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public TicketsController(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    // GET /api/tickets
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll()
    {
        var tickets = await _context.Tickets.Include(t => t.Event).ToListAsync();
        return Ok(_mapper.Map<List<TicketResponseDto>>(tickets));
    }

    // GET /api/tickets/{id}
    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetById(int id)
    {
        var ticket = await _context.Tickets.Include(t => t.Event).FirstOrDefaultAsync(t => t.Id == id);
        if (ticket == null) return NotFound();
        return Ok(_mapper.Map<TicketResponseDto>(ticket));
    }

    // POST /api/tickets
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] TicketCreateDto dto)
    {
        var eventExists = await _context.Events.AnyAsync(e => e.Id == dto.EventId);
        if (!eventExists)
            return BadRequest(new { error = "Event with the specified ID does not exist." });

        var ticket = _mapper.Map<Ticket>(dto);
        _context.Tickets.Add(ticket);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = ticket.Id }, _mapper.Map<TicketResponseDto>(ticket));
    }

    // PUT /api/tickets/{id}
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] TicketUpdateDto dto)
    {
        var ticket = await _context.Tickets.FindAsync(id);
        if (ticket == null) return NotFound();

        var eventExists = await _context.Events.AnyAsync(e => e.Id == dto.EventId);
        if (!eventExists)
            return BadRequest(new { error = "Event with the specified ID does not exist." });

        _mapper.Map(dto, ticket);
        await _context.SaveChangesAsync();
        return Ok(_mapper.Map<TicketResponseDto>(ticket));
    }

    // DELETE /api/tickets/{id}
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var ticket = await _context.Tickets.FindAsync(id);
        if (ticket == null) return NotFound();
        _context.Tickets.Remove(ticket);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}