using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EventHub.Web.Models.DTOs;
using EventHub.Web.Services;

namespace EventHub.Web.Controllers;

public class EventsController : Controller
{
    private readonly IEventService _eventService;
    private readonly ILogger<EventsController> _logger;

    public EventsController(IEventService eventService, ILogger<EventsController> logger)
    {
        _eventService = eventService;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var events = await _eventService.GetAllEventsAsync();
            return View(events);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading events");
            return RedirectToAction("Error", "Home");
        }
    }

    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var @event = await _eventService.GetEventByIdAsync(id);
            if (@event == null)
                return NotFound();

            // Load tickets and store in ViewBag
            var tickets = await _eventService.GetTicketsByEventIdAsync(id);
            ViewBag.Tickets = tickets;

            return View(@event);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading event {Id}", id);
            return RedirectToAction("Error", "Home");
        }
    }

    // ─── Admin Actions ────────────────────────────────────────────────────────

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create()
    {
        ViewBag.Organizers = await _eventService.GetAllOrganizersAsync();
        return View();
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EventCreateDto dto)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Organizers = await _eventService.GetAllOrganizersAsync();
            return View(dto);
        }

        var created = await _eventService.CreateEventAsync(dto);
        if (created == null)
        {
            ModelState.AddModelError(string.Empty, "Failed to create event. Please try again.");
            return View(dto);
        }

        TempData["SuccessMessage"] = "Event created successfully!";
        return RedirectToAction(nameof(Details), new { id = created.Id });
    }

    [Authorize(Roles = "Admin")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id)
    {
        var @event = await _eventService.GetEventByIdAsync(id);
        if (@event == null)
            return NotFound();

        var dto = new EventUpdateDto
        {
            Title = @event.Title,
            Description = @event.Description,
            Date = @event.Date,
            Location = @event.Location,
            BannerImageUrl = @event.BannerImageUrl,
            OrganizerId = @event.OrganizerId
        };

        ViewBag.EventId = id;
        ViewBag.OrganizerName = @event.Organizer?.Name;
        ViewBag.Organizers = await _eventService.GetAllOrganizersAsync();
        return View(dto);
    }
    //[Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EventUpdateDto dto)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.EventId = id;
            ViewBag.Organizers = await _eventService.GetAllOrganizersAsync();
            return View(dto);
        }

        var updated = await _eventService.UpdateEventAsync(id, dto);
        if (updated == null)
        {
            ModelState.AddModelError(string.Empty, "Failed to update event. Please try again.");
            ViewBag.EventId = id;
            return View(dto);
        }

        TempData["SuccessMessage"] = "Event updated successfully!";
        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _eventService.DeleteEventAsync(id);
        if (!success)
        {
            TempData["ErrorMessage"] = "Failed to delete event.";
            return RedirectToAction(nameof(Details), new { id });
        }

        TempData["SuccessMessage"] = "Event deleted successfully!";
        return RedirectToAction(nameof(Index));
    }
    public IActionResult CreateOrganizer()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateOrganizer(OrganizerCreateDto dto)
    {
        if (!ModelState.IsValid) return View(dto);

        var created = await _eventService.CreateOrganizerAsync(dto);
        if (created == null)
        {
            ModelState.AddModelError(string.Empty, "Failed to create organizer.");
            return View(dto);
        }

        TempData["SuccessMessage"] = $"Organizer '{created.Name}' created!";
        return RedirectToAction(nameof(Create));
    }

    public async Task<IActionResult> CreateTicket(int eventId)
    {
        var ev = await _eventService.GetEventByIdAsync(eventId);
        if (ev == null) return NotFound();
        ViewBag.Event = ev;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateTicket(int eventId, TicketCreateDto dto)
    {
        var ev = await _eventService.GetEventByIdAsync(eventId);
        if (!ModelState.IsValid)
        {
            ViewBag.Event = ev;
            return View(dto);
        }

        var created = await _eventService.CreateTicketAsync(eventId, dto);
        if (created == null)
        {
            ModelState.AddModelError(string.Empty, "Failed to create ticket.");
            ViewBag.Event = ev;
            return View(dto);
        }

        TempData["SuccessMessage"] = "Ticket added!";
        return RedirectToAction(nameof(Details), new { id = eventId });
    }
}