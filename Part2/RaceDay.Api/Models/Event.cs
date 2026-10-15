using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Api.Models;

// A race created by an Organiser
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

    // 2 means Open, a new event starts as Open
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

    // In kilometres
    [Column("DISTANCE", TypeName = "decimal(10,2)")]
    public decimal Distance { get; set; }

    // Link to the banner image in Azure Blob Storage (used in Part 3)
    [Column("BANNER_URL")]
    [MaxLength(500)]
    public string? BannerUrl { get; set; }

    // Related rows, filled in when we use Include
    public User? Organiser { get; set; }
    public EventType? EventType { get; set; }
    public EventStatus? EventStatus { get; set; }
    public List<Category> Categories { get; set; } = new();
    public List<Enrolment> Enrolments { get; set; } = new();
}
