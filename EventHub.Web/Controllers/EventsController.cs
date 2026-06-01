using Microsoft.AspNetCore.Mvc;
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
            _logger.LogError($"Error loading events: {ex.Message}");
            return RedirectToAction("Error", "Home");
        }
    }

    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var @event = await _eventService.GetEventByIdAsync(id);
            if (@event == null)
            {
                return NotFound();
            }
            return View(@event);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error loading event {id}: {ex.Message}");
            return RedirectToAction("Error", "Home");
        }
    }
}

