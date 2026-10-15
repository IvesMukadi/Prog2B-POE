using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Api.Models;

// Links a participant to an event and the category they picked
[Table("Enrolments")]
public class Enrolment
{
    [Key]
    [Column("ENROLMENT_ID")]
    public int EnrolmentId { get; set; }

    [Column("USER_ID")]
    public int UserId { get; set; }

    [Column("EVENT_ID")]
    public int EventId { get; set; }

    [Column("CATEGORY_ID")]
    public int CategoryId { get; set; }

    // 1 means Pending, a new enrolment starts as Pending
    [Column("STATUS_ID")]
    public int StatusId { get; set; } = 1;

    public User? User { get; set; }
    public Event? Event { get; set; }
    public Category? Category { get; set; }
    public EnrolmentStatus? Status { get; set; }
    public List<Result> Results { get; set; } = new();
}
