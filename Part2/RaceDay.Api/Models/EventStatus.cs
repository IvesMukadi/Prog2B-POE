using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Api.Models;

// Lookup table, for example Draft, Open or Completed
[Table("Event_Status")]
public class EventStatus
{
    [Key]
    [Column("EVENT_STATUS_ID")]
    public int EventStatusId { get; set; }

    [Column("STATUS_NAME")]
    [MaxLength(50)]
    public string StatusName { get; set; } = "";
}
