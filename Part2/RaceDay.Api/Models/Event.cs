using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Api.Models;

[Table("Events")]
public class Event
{
    [Key]
    [Column("EVENT_ID")]
    public int EventId { get; set; }

    [Column("USER_ID")]
    public int UserId { get; set; }

    [Column("TYPE_ID")]
    public int TypeId { get; set; }

    [Column("EVENT_STATUS_ID")]
    public int EventStatusId { get; set; } = 2;

    [Column("NAME")]
    [MaxLength(100)]
    public string Name { get; set; } = "";

    [Column("DESCRIPTION")]
    [MaxLength(500)]
    public string Description { get; set; } = "";

    [Column("EVENT_DATE", TypeName = "datetime")]
    public DateTime EventDate { get; set; }

    [Column("LOCATION")]
    [MaxLength(200)]
    public string Location { get; set; } = "";

    [Column("DISTANCE", TypeName = "decimal(10,2)")]
    public decimal Distance { get; set; }

    [Column("BANNER_URL")]
    [MaxLength(500)]
    public string? BannerUrl { get; set; }

    public User? Organiser { get; set; }
    public EventType? EventType { get; set; }
    public EventStatus? EventStatus { get; set; }
    public List<Category> Categories { get; set; } = new();
    public List<Enrolment> Enrolments { get; set; } = new();
}
