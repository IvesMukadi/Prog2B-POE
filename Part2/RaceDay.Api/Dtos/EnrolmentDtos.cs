using System.ComponentModel.DataAnnotations;
using RaceDay.Api.Models;

namespace RaceDay.Api.Dtos;

public class EnrolDto
{
    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }
}

public class StatusDto
{
    [Range(1, int.MaxValue)]
    public int StatusId { get; set; }
}

public class EnrolmentDto
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

    // The enrolment needs its user, event, category and status loaded
    public static EnrolmentDto FromEnrolment(Enrolment e)
    {
        return new EnrolmentDto
        {
            EnrolmentId = e.EnrolmentId,
            EventId = e.EventId,
            EventName = e.Event?.Name ?? "",
            EventDate = e.Event?.EventDate ?? default,
            UserId = e.UserId,
            ParticipantName = e.User == null ? "" : e.User.FirstName + " " + e.User.LastName,
            CategoryId = e.CategoryId,
            CategoryName = e.Category?.CategoryName ?? "",
            StatusId = e.StatusId,
            StatusName = e.Status?.StatusName ?? ""
        };
    }
}
