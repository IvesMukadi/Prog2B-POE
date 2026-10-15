using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly RaceDayContext _context;

    public AuthController(RaceDayContext context)
    {
        _context = context;
    }

    /// <summary>Registers a new Organiser or Participant account.</summary>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserDto>> Register(RegisterDto dto)
    {
        // Emails are saved in lower case so they are easy to compare
        var email = dto.Email.Trim().ToLower();

        var emailTaken = await _context.Users.AnyAsync(u => u.Email == email);
        if (emailTaken)
        {
            return BadRequest("That email is already registered.");
        }

        var user = new User
        {
            Email = email,
            Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Role = dto.Role
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Created("/api/users/me", UserDto.FromUser(user));
    }
}
