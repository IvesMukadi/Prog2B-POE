using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Api.Models;

// A group inside an event that a participant can enter, like Elite Men
[Table("Categories")]
public class Category
{
    [Key]
    [Column("CATEGORY_ID")]
    public int CategoryId { get; set; }

    [Column("EVENT_ID")]
    public int EventId { get; set; }

    [Column("CATEGORY_NAME")]
    [MaxLength(100)]
    public string CategoryName { get; set; } = "";

    [Column("AGE_GROUP")]
    [MaxLength(50)]
    public string? AgeGroup { get; set; }

    public Event? Event { get; set; }
}
