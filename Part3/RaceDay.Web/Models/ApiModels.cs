namespace RaceDay.Web.Models;

// These classes match the JSON the API sends back

public class UserInfo
{
    public int UserId { get; set; }
    public string Email { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Role { get; set; } = "";
    public string? ProfilePictureUrl { get; set; }
}

public class CategoryInfo
{
    public int CategoryId { get; set; }
    public int EventId { get; set; }
    public string CategoryName { get; set; } = "";
    public string? AgeGroup { get; set; }
}

public class EventInfo
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
    public List<CategoryInfo>? Categories { get; set; }
}

public class EnrolmentInfo
{
    public int EnrolmentId { get; set; }
    public int EventId { get; set; }
    public string EventName { get; set; } = "";
    public DateTime EventDate { get; set; }
    public int UserId { get; set; }
    public string ParticipantName { get; set; } = "";
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = "";
    public int StatusId { get; set; }
    public string StatusName { get; set; } = "";
}

public class ResultInfo
{
    public int ResultId { get; set; }
    public int EnrolmentId { get; set; }
    public int EventId { get; set; }
    public string EventName { get; set; } = "";
    public DateTime EventDate { get; set; }
    public string CategoryName { get; set; } = "";
    public string ParticipantName { get; set; } = "";
    public TimeSpan FinishTime { get; set; }
    public int FinishingPosition { get; set; }
    public int TotalFinishers { get; set; }
}
