using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Api.Models;

[Table("Users")]
public class User
{
    [Key]
    [Column("USER_ID")]
    public int UserId { get; set; }

    [Column("EMAIL")]
    [MaxLength(100)]
    public string Email { get; set; } = "";

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

    [Column("PROFILE_PICTURE_URL")]
    [MaxLength(500)]
    public string? ProfilePictureUrl { get; set; }
}
