using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Api.Models;

// A person who uses the system, either an Organiser or a Participant
[Table("Users")]
public class User
{
    [Key]
    [Column("USER_ID")]
    public int UserId { get; set; }

    [Column("EMAIL")]
    [MaxLength(100)]
    public string Email { get; set; } = "";

    // Saved as a BCrypt hash, never as plain text
    [Column("PASSWORD")]
    [MaxLength(255)]
    public string Password { get; set; } = "";

    [Column("FIRST_NAME")]
    [MaxLength(50)]
    public string FirstName { get; set; } = "";

    [Column("LAST_NAME")]
    [MaxLength(50)]
    public string LastName { get; set; } = "";

    [Column("ROLE")]
    [MaxLength(20)]
    public string Role { get; set; } = "";

    // Link to the picture in Azure Blob Storage (used in Part 3)
    [Column("PROFILE_PICTURE_URL")]
    [MaxLength(500)]
    public string? ProfilePictureUrl { get; set; }
}
