using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Api.Models;

[Table("Event_Types")]
public class EventType
{
    [Key]
    [Column("TYPE_ID")]
    public int TypeId { get; set; }

    [Column("TYPE_NAME")]
    [MaxLength(50)]
    public string TypeName { get; set; } = "";
}
