using System.ComponentModel.DataAnnotations;
using RaceDay.Api.Models;

namespace RaceDay.Api.Dtos;

public class RegisterDto
{
    [Required, EmailAddress, MaxLength(100)]
    public string Email { get; set; } = "";

    [Required, MinLength(6), MaxLength(100)]
    public string Password { get; set; } = "";

    [Required, MaxLength(50)]
    public string FirstName { get; set; } = "";

    [Required, MaxLength(50)]
    public string LastName { get; set; } = "";

    // Only these two roles are allowed
    [Required, RegularExpression("^(Organiser|Participant)$", ErrorMessage = "Role must be Organiser or Participant.")]
    public string Role { get; set; } = "";
}

public class LoginDto
{
    [Required]
    public string Email { get; set; } = "";

    [Required]
    public string Password { get; set; } = "";
}

public class UpdateProfileDto
{
    [Required, EmailAddress, MaxLength(100)]
    public string Email { get; set; } = "";

    [Required, MaxLength(50)]
    public string FirstName { get; set; } = "";

    [Required, MaxLength(50)]
    public string LastName { get; set; } = "";
}

// Sent back to the client, it leaves out the password
public class UserDto
{
    public int UserId { get; set; }
    public string Email { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Role { get; set; } = "";
    public string? ProfilePictureUrl { get; set; }

    public static UserDto FromUser(User user)
    {
        return new UserDto
        {
            UserId = user.UserId,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role,
            ProfilePictureUrl = user.ProfilePictureUrl
        };
    }
}
