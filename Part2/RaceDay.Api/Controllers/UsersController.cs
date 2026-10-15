using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Helpers;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/users/me")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly RaceDayContext _context;

    public UsersController(RaceDayContext context)
    {
        _context = context;
    }

    /// <summary>Gets the profile of the logged in user.</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserDto>> GetMe()
    {
        var user = await _context.Users.FindAsync(User.GetUserId());
        if (user == null)
        {
            return NotFound();
        }

        return UserDto.FromUser(user);
    }

    /// <summary>Updates the name and email of the logged in user.</summary>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserDto>> UpdateMe(UpdateProfileDto dto)
    {
        var user = await _context.Users.FindAsync(User.GetUserId());
        if (user == null)
        {
            return NotFound();
        }

        // Another user must not already have this email
        var email = dto.Email.Trim().ToLower();
        var emailTaken = await _context.Users.AnyAsync(u => u.Email == email && u.UserId != user.UserId);
        if (emailTaken)
        {
            return BadRequest("That email is already registered.");
        }

        user.Email = email;
        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        await _context.SaveChangesAsync();

        return UserDto.FromUser(user);
    }
}
