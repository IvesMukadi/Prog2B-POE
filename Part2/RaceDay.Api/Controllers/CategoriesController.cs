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
public class CategoriesController : ControllerBase
{
    private readonly RaceDayContext _context;

    public CategoriesController(RaceDayContext context)
    {
        _context = context;
    }

    /// <summary>Gets all categories for an event.</summary>
    [HttpGet("events/{eventId:int}/categories")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<CategoryDto>>> GetCategories(int eventId)
    {
        var eventExists = await _context.Events.AnyAsync(e => e.EventId == eventId);
        if (!eventExists)
        {
            return NotFound();
        }

        var categories = await _context.Categories
            .Where(c => c.EventId == eventId)
            .OrderBy(c => c.CategoryId)
            .ToListAsync();

        return categories.Select(CategoryDto.FromCategory).ToList();
    }

    /// <summary>Adds a category to an event (only the organiser who created it).</summary>
    [HttpPost("events/{eventId:int}/categories")]
    [Authorize(Roles = "Organiser")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoryDto>> AddCategory(int eventId, SaveCategoryDto dto)
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

        // Two categories in one event cannot have the same name
        var nameTaken = await _context.Categories.AnyAsync(c => c.EventId == eventId && c.CategoryName == dto.CategoryName);
        if (nameTaken)
        {
            return BadRequest("This event already has a category with that name.");
        }

        var category = new Category
        {
            EventId = eventId,
            CategoryName = dto.CategoryName,
            AgeGroup = dto.AgeGroup
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCategories), new { eventId }, CategoryDto.FromCategory(category));
    }

    /// <summary>Updates a category (only the organiser who created the event).</summary>
    [HttpPut("categories/{id:int}")]
    [Authorize(Roles = "Organiser")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoryDto>> UpdateCategory(int id, SaveCategoryDto dto)
    {
        var category = await _context.Categories.Include(c => c.Event).FirstOrDefaultAsync(c => c.CategoryId == id);
        if (category == null)
        {
            return NotFound();
        }
        if (category.Event!.UserId != User.GetUserId())
        {
            return Forbid();
        }

        var nameTaken = await _context.Categories.AnyAsync(c => c.EventId == category.EventId && c.CategoryName == dto.CategoryName && c.CategoryId != id);
        if (nameTaken)
        {
            return BadRequest("This event already has a category with that name.");
        }

        category.CategoryName = dto.CategoryName;
        category.AgeGroup = dto.AgeGroup;
        await _context.SaveChangesAsync();

        return CategoryDto.FromCategory(category);
    }

    /// <summary>Deletes a category that has no enrolments (only the organiser who created the event).</summary>
    [HttpDelete("categories/{id:int}")]
    [Authorize(Roles = "Organiser")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var category = await _context.Categories.Include(c => c.Event).FirstOrDefaultAsync(c => c.CategoryId == id);
        if (category == null)
        {
            return NotFound();
        }
        if (category.Event!.UserId != User.GetUserId())
        {
            return Forbid();
        }

        // The database would block this delete, so we check first and give a clear message
        var hasEnrolments = await _context.Enrolments.AnyAsync(e => e.CategoryId == id);
        if (hasEnrolments)
        {
            return BadRequest("This category has enrolments and cannot be deleted.");
        }

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
