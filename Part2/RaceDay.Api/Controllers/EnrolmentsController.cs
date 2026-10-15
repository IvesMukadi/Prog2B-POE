using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Helpers;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api")]
public class EnrolmentsController : ControllerBase
{
    private readonly RaceDayContext _context;

    public EnrolmentsController(RaceDayContext context)
    {
        _context = context;
    }

    // Enrolments with the user, event, category and status loaded
    private IQueryable<Enrolment> EnrolmentsWithDetails()
    {
        return _context.Enrolments
            .Include(e => e.User)
            .Include(e => e.Event)
            .Include(e => e.Category)
            .Include(e => e.Status);
    }

    /// <summary>Enrols the logged in participant in an event by choosing a category.</summary>
    [HttpPost("events/{eventId:int}/enrol")]
    [Authorize(Roles = "Participant")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EnrolmentDto>> Enrol(int eventId, EnrolDto dto)
    {
        var ev = await _context.Events.FindAsync(eventId);
        if (ev == null)
        {
            return NotFound();
        }

        // Event status 2 is Open
        if (ev.EventStatusId != 2)
        {
            return BadRequest("This event is not open for enrolment.");
        }

        // The category must be one of this event's categories
        var categoryBelongsToEvent = await _context.Categories.AnyAsync(c => c.CategoryId == dto.CategoryId && c.EventId == eventId);
        if (!categoryBelongsToEvent)
        {
            return BadRequest("That category does not belong to this event.");
        }

        var userId = User.GetUserId();
        var alreadyEnrolled = await _context.Enrolments.AnyAsync(e => e.UserId == userId && e.EventId == eventId);
        if (alreadyEnrolled)
        {
            return BadRequest("You are already enrolled in this event.");
        }

        // The status starts as Pending (the default)
        var enrolment = new Enrolment
        {
            UserId = userId,
            EventId = eventId,
            CategoryId = dto.CategoryId
        };

        _context.Enrolments.Add(enrolment);
        await _context.SaveChangesAsync();

        var created = await EnrolmentsWithDetails().FirstAsync(e => e.EnrolmentId == enrolment.EnrolmentId);
        return CreatedAtAction(nameof(GetMyEnrolments), EnrolmentDto.FromEnrolment(created));
    }

    /// <summary>Gets the logged in participant's enrolments.</summary>
    [HttpGet("users/me/enrolments")]
    [Authorize(Roles = "Participant")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<EnrolmentDto>>> GetMyEnrolments()
    {
        var userId = User.GetUserId();
        var enrolments = await EnrolmentsWithDetails()
            .Where(e => e.UserId == userId)
            .OrderBy(e => e.Event!.EventDate)
            .ToListAsync();

        return enrolments.Select(EnrolmentDto.FromEnrolment).ToList();
    }

    /// <summary>Gets all enrolments for an event (only the organiser who created it).</summary>
    [HttpGet("events/{eventId:int}/enrolments")]
    [Authorize(Roles = "Organiser")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<EnrolmentDto>>> GetEventEnrolments(int eventId)
    {
        var ev = await _context.Events.FindAsync(eventId);
        if (ev == null)
        {
            return NotFound();
        }
        if (ev.UserId != User.GetUserId())
        {
            return Forbid();
        }

        var enrolments = await EnrolmentsWithDetails()
            .Where(e => e.EventId == eventId)
            .OrderBy(e => e.EnrolmentId)
            .ToListAsync();

        return enrolments.Select(EnrolmentDto.FromEnrolment).ToList();
    }

    /// <summary>Changes the status of an enrolment (only the organiser who created the event).</summary>
    [HttpPut("enrolments/{id:int}/status")]
    [Authorize(Roles = "Organiser")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EnrolmentDto>> UpdateStatus(int id, StatusDto dto)
    {
        var enrolment = await EnrolmentsWithDetails().FirstOrDefaultAsync(e => e.EnrolmentId == id);
        if (enrolment == null)
        {
            return NotFound();
        }
        if (enrolment.Event!.UserId != User.GetUserId())
        {
            return Forbid();
        }

        var status = await _context.EnrolmentStatuses.FindAsync(dto.StatusId);
        if (status == null)
        {
            return BadRequest("Status does not exist.");
        }

        enrolment.StatusId = status.StatusId;
        enrolment.Status = status;
        await _context.SaveChangesAsync();

        return EnrolmentDto.FromEnrolment(enrolment);
    }
}
