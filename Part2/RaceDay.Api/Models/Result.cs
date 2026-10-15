using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Api.Models;

// The finish time and position of one enrolment
[Table("Results")]
public class Result
{
    [Key]
    [Column("RESULT_ID")]
    public int ResultId { get; set; }

    [Column("ENROLMENT_ID")]
    public int EnrolmentId { get; set; }

    [Column("FINISH_TIME", TypeName = "time")]
    public TimeSpan FinishTime { get; set; }

    [Column("FINISHING_POSITION")]
    public int FinishingPosition { get; set; }

    public Enrolment? Enrolment { get; set; }
}
