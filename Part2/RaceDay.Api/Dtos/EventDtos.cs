using System.ComponentModel.DataAnnotations;
using RaceDay.Api.Models;

namespace RaceDay.Api.Dtos;

public class CreateEventDto
{
    /// <example>Soweto 10km</example>
    [Required, MaxLength(100)]
    public string Name { get; set; } = "";

    /// <example>A fun run through Soweto</example>
    [Required, MaxLength(500)]
    public string Description { get; set; } = "";

    /// <example>2027-03-20T07:00:00</example>
    [Range(typeof(DateTime), "2000-01-01", "2100-12-31")]
    public DateTime Date { get; set; }

    /// <example>Orlando Stadium, Soweto</example>
    [Required, MaxLength(200)]
    public string Location { get; set; } = "";

    // Distance is in kilometres
    /// <example>10</example>
    [Range(0.01, 1000)]
    public decimal Distance { get; set; }

    /// <example>1</example>
    [Range(1, int.MaxValue)]
    public int EventTypeId { get; set; }
}

// An update is the same as a create plus the event status
public class UpdateEventDto : CreateEventDto
{
    /// <example>2</example>
    [Range(1, int.MaxValue)]
    public int EventStatusId { get; set; }
}

public class EventDto
{
    public int EventId { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public DateTime Date { get; set; }
    public string Location { get; set; } = "";
    public decimal Distance { get; set; }
    public string? BannerUrl { get; set; }
    public int EventTypeId { get; set; }
    public string EventType { get; set; } = "";
    public int EventStatusId { get; set; }
    public string EventStatus { get; set; } = "";
    public string OrganiserName { get; set; } = "";
    public List<CategoryDto>? Categories { get; set; }

    // Categories are only included when asked for (event details page)
    public static EventDto FromEvent(Event e, bool withCategories = false)
    {
        return new EventDto
        {
            EventId = e.EventId,
            Name = e.Name,
            Description = e.Description,
            Date = e.EventDate,
            Location = e.Location,
            Distance = e.Distance,
            BannerUrl = e.BannerUrl,
            EventTypeId = e.TypeId,
            EventType = e.EventType?.TypeName ?? "",
            EventStatusId = e.EventStatusId,
            EventStatus = e.EventStatus?.StatusName ?? "",
            OrganiserName = e.Organiser == null ? "" : e.Organiser.FirstName + " " + e.Organiser.LastName,
            Categories = withCategories ? e.Categories.Select(CategoryDto.FromCategory).ToList() : null
        };
    }
}
