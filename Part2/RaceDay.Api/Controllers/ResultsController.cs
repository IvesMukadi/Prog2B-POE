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
public class ResultsController : ControllerBase
{
    private readonly RaceDayContext _context;

    public ResultsController(RaceDayContext context)
    {
        _context = context;
    }

    // Results with the enrolment, event, category and participant loaded
    private IQueryable<Result> ResultsWithDetails()
    {
        return _context.Results
            .Include(r => r.Enrolment!).ThenInclude(e => e.Event)
            .Include(r => r.Enrolment!).ThenInclude(e => e.Category)
            .Include(r => r.Enrolment!).ThenInclude(e => e.User);
    }

    /// <summary>Records a finish time and position for an enrolment (only the organiser who created the event).</summary>
    [HttpPost("events/{eventId:int}/results")]
    [Authorize(Roles = "Organiser")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ResultDto>> AddResult(int eventId, CreateResultDto dto)
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

        // Event status 4 is Completed
        if (ev.EventStatusId != 4)
        {
            return BadRequest("Results can only be added once the event is completed.");
        }

        // The enrolment must be for this event
        var enrolmentInEvent = await _context.Enrolments.AnyAsync(e => e.EnrolmentId == dto.EnrolmentId && e.EventId == eventId);
        if (!enrolmentInEvent)
        {
            return BadRequest("That enrolment does not belong to this event.");
        }

        // One result per enrolment
        var alreadyHasResult = await _context.Results.AnyAsync(r => r.EnrolmentId == dto.EnrolmentId);
        if (alreadyHasResult)
        {
            return BadRequest("This enrolment already has a result.");
        }

        var result = new Result
        {
            EnrolmentId = dto.EnrolmentId,
            FinishTime = dto.FinishTime,
            FinishingPosition = dto.FinishingPosition
        };

        _context.Results.Add(result);
        await _context.SaveChangesAsync();

        var created = await ResultsWithDetails().FirstAsync(r => r.ResultId == result.ResultId);
        var totalFinishers = await _context.Results.CountAsync(r => r.Enrolment!.EventId == eventId);
        return CreatedAtAction(nameof(GetEventResults), new { eventId }, ResultDto.FromResult(created, totalFinishers));
    }

    /// <summary>Gets the logged in participant's results across all events.</summary>
    [HttpGet("users/me/results")]
    [Authorize(Roles = "Participant")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<ResultDto>>> GetMyResults()
    {
        var userId = User.GetUserId();
        var results = await ResultsWithDetails()
            .Where(r => r.Enrolment!.UserId == userId)
            .OrderByDescending(r => r.Enrolment!.Event!.EventDate)
            .ToListAsync();

        // Count the finishers of each event for the "out of" number
        var dtos = new List<ResultDto>();
        foreach (var result in results)
        {
            var eventId = result.Enrolment!.EventId;
            var totalFinishers = await _context.Results.CountAsync(r => r.Enrolment!.EventId == eventId);
            dtos.Add(ResultDto.FromResult(result, totalFinishers));
        }

        return dtos;
    }

    /// <summary>Gets all results for an event (only the organiser who created it).</summary>
    [HttpGet("events/{eventId:int}/results")]
    [Authorize(Roles = "Organiser")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<ResultDto>>> GetEventResults(int eventId)
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

        var results = await ResultsWithDetails()
            .Where(r => r.Enrolment!.EventId == eventId)
            .OrderBy(r => r.FinishingPosition)
            .ToListAsync();

        return results.Select(r => ResultDto.FromResult(r, results.Count)).ToList();
    }
}
