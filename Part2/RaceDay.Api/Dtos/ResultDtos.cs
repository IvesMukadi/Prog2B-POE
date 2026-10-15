using System.ComponentModel.DataAnnotations;
using RaceDay.Api.Models;

namespace RaceDay.Api.Dtos;

public class CreateResultDto
{
    [Range(1, int.MaxValue)]
    public int EnrolmentId { get; set; }

    // Sent as text like "02:05:40"
    public TimeSpan FinishTime { get; set; }

    [Range(1, int.MaxValue)]
    public int FinishingPosition { get; set; }
}

public class ResultDto
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

    // Used to show "34th out of 2 finishers"
    public int TotalFinishers { get; set; }

    // The result needs its enrolment, event, category and user loaded
    public static ResultDto FromResult(Result r, int totalFinishers)
    {
        var enrolment = r.Enrolment;
        return new ResultDto
        {
            ResultId = r.ResultId,
            EnrolmentId = r.EnrolmentId,
            EventId = enrolment?.EventId ?? 0,
            EventName = enrolment?.Event?.Name ?? "",
            EventDate = enrolment?.Event?.EventDate ?? default,
            CategoryName = enrolment?.Category?.CategoryName ?? "",
            ParticipantName = enrolment?.User == null ? "" : enrolment.User.FirstName + " " + enrolment.User.LastName,
            FinishTime = r.FinishTime,
            FinishingPosition = r.FinishingPosition,
            TotalFinishers = totalFinishers
        };
    }
}
