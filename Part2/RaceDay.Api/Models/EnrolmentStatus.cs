using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Api.Models;

// Lookup table, for example Pending, Confirmed or Cancelled
[Table("Enrolment_Status")]
public class EnrolmentStatus
{
    [Key]
    [Column("STATUS_ID")]
    public int StatusId { get; set; }

    [Column("STATUS_NAME")]
    [MaxLength(50)]
    public string StatusName { get; set; } = "";
}
