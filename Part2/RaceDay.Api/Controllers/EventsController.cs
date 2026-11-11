using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Helpers;
using RaceDay.Api.Models;
using RaceDay.Api.Services;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/events")]
public class EventsController : ControllerBase
{
    private readonly RaceDayContext _context;
    private readonly IBlobService _blob;

    public EventsController(RaceDayContext context, IBlobService blob)
    {
        _context = context;
        _blob = blob;
    }

    // Events with their type, status and organiser loaded
    private IQueryable<Event> EventsWithDetails()
    {
        return _context.Events
            .Include(e => e.EventType)
            .Include(e => e.EventStatus)
            .Include(e => e.Organiser);
    }

    /// <summary>Gets all events. Optional filters: type id, status id and date.</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<EventDto>>> GetEvents(int? type, int? status, DateTime? date)
    {
        var query = EventsWithDetails();

        // Only filter when the caller sent a value
        if (type != null)
        {
            query = query.Where(e => e.TypeId == type);
        }
        if (status != null)
        {
            query = query.Where(e => e.EventStatusId == status);
        }
        if (date != null)
        {
            query = query.Where(e => e.EventDate.Date == date.Value.Date);
        }

        var events = await query.OrderBy(e => e.EventDate).ToListAsync();
        return events.Select(e => EventDto.FromEvent(e)).ToList();
    }

    /// <summary>Gets one event with its categories.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventDto>> GetEvent(int id)
    {
        var ev = await EventsWithDetails()
            .Include(e => e.Categories)
            .FirstOrDefaultAsync(e => e.EventId == id);

        if (ev == null)
        {
            return NotFound();
        }

        return EventDto.FromEvent(ev, true);
    }

    /// <summary>Gets the events created by the logged in organiser.</summary>
    [HttpGet("mine")]
    [Authorize(Roles = "Organiser")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<EventDto>>> GetMyEvents()
    {
        var userId = User.GetUserId();
        var events = await EventsWithDetails()
            .Where(e => e.UserId == userId)
            .OrderBy(e => e.EventDate)
            .ToListAsync();

        return events.Select(e => EventDto.FromEvent(e)).ToList();
    }

    /// <summary>Creates a new event (Organiser only).</summary>
    [HttpPost]
    [Authorize(Roles = "Organiser")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<EventDto>> CreateEvent(CreateEventDto dto)
    {
        var typeExists = await _context.EventTypes.AnyAsync(t => t.TypeId == dto.EventTypeId);
        if (!typeExists)
        {
            return BadRequest("Event type does not exist.");
        }

        var ev = new Event
        {
            UserId = User.GetUserId(),
            TypeId = dto.EventTypeId,
            Name = dto.Name,
            Description = dto.Description,
            EventDate = dto.Date,
            Location = dto.Location,
            Distance = dto.Distance
        };

        _context.Events.Add(ev);
        await _context.SaveChangesAsync();

        // Load it again so the type, status and organiser names are filled in
        var created = await EventsWithDetails().FirstAsync(e => e.EventId == ev.EventId);
        return CreatedAtAction(nameof(GetEvent), new { id = ev.EventId }, EventDto.FromEvent(created));
    }

    /// <summary>Updates an event (only the organiser who created it).</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Organiser")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventDto>> UpdateEvent(int id, UpdateEventDto dto)
    {
        var ev = await _context.Events.FindAsync(id);
        if (ev == null)
        {
            return NotFound();
        }

        // An organiser can only change their own events
        if (ev.UserId != User.GetUserId())
        {
            return Forbid();
        }

        var typeExists = await _context.EventTypes.AnyAsync(t => t.TypeId == dto.EventTypeId);
        var statusExists = await _context.EventStatuses.AnyAsync(s => s.EventStatusId == dto.EventStatusId);
        if (!typeExists || !statusExists)
        {
            return BadRequest("Event type or status does not exist.");
        }

        ev.Name = dto.Name;
        ev.Description = dto.Description;
        ev.EventDate = dto.Date;
        ev.Location = dto.Location;
        ev.Distance = dto.Distance;
        ev.TypeId = dto.EventTypeId;
        ev.EventStatusId = dto.EventStatusId;
        await _context.SaveChangesAsync();

        var updated = await EventsWithDetails().FirstAsync(e => e.EventId == id);
        return EventDto.FromEvent(updated);
    }

    /// <summary>Uploads a banner image for an event (only the organiser who created it).</summary>
    [HttpPost("{id:int}/banner")]
    [Authorize(Roles = "Organiser")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventDto>> UploadBanner(int id, IFormFile? file)
    {
        var ev = await _context.Events.FindAsync(id);
        if (ev == null)
        {
            return NotFound();
        }

        // An organiser can only change their own events
        if (ev.UserId != User.GetUserId())
        {
            return Forbid();
        }

        var error = ImageCheck.GetError(file);
        if (error != null)
        {
            return BadRequest(error);
        }

        // A random name stops one upload replacing another
        var fileName = $"event-{id}-{Guid.NewGuid()}{Path.GetExtension(file!.FileName).ToLower()}";
        using var stream = file.OpenReadStream();
        ev.BannerUrl = await _blob.UploadAsync(stream, "banners", fileName, file.ContentType);
        await _context.SaveChangesAsync();

        var updated = await EventsWithDetails().FirstAsync(e => e.EventId == id);
        return EventDto.FromEvent(updated);
    }

    /// <summary>Deletes an event and its categories, enrolments and results (only the organiser who created it).</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Organiser")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        var ev = await _context.Events.FindAsync(id);
        if (ev == null)
        {
            return NotFound();
        }
        if (ev.UserId != User.GetUserId())
        {
            return Forbid();
        }

        // The database deletes the categories, enrolments and results too
        _context.Events.Remove(ev);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
