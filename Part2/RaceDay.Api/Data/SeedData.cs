using RaceDay.Api.Models;

namespace RaceDay.Api.Data;

public static class SeedData
{
    public static void Initialize(RaceDayContext db)
    {
        // Skip if the database already has data
        if (db.EventStatuses.Any())
        {
            return;
        }

        // Lookup tables first because the other tables point to them
        var open = new EventStatus { StatusName = "Open" };
        var completed = new EventStatus { StatusName = "Completed" };
        db.EventStatuses.AddRange(
            new EventStatus { StatusName = "Draft" }, open,
            new EventStatus { StatusName = "Closed" }, completed,
            new EventStatus { StatusName = "Cancelled" });

        var confirmed = new EnrolmentStatus { StatusName = "Confirmed" };
        db.EnrolmentStatuses.AddRange(
            new EnrolmentStatus { StatusName = "Pending" }, confirmed,
            new EnrolmentStatus { StatusName = "Cancelled" },
            new EnrolmentStatus { StatusName = "Completed" },
            new EnrolmentStatus { StatusName = "Waitlisted" });

        var run = new EventType { TypeName = "Run" };
        var cycle = new EventType { TypeName = "Cycle" };
        db.EventTypes.AddRange(run, new EventType { TypeName = "Walk" }, cycle,
            new EventType { TypeName = "Triathlon" }, new EventType { TypeName = "Ultra Marathon" });
        db.SaveChanges();

        // Every seeded user logs in with Password123!
        var password = BCrypt.Net.BCrypt.HashPassword("Password123!");
        var thabo = new User { Email = "thabo@raceday.co.za", Password = password, FirstName = "Thabo", LastName = "Mokoena", Role = "Organiser" };
        var priya = new User { Email = "priya@raceday.co.za", Password = password, FirstName = "Priya", LastName = "Naidoo", Role = "Organiser" };
        var amara = new User { Email = "amara@raceday.co.za", Password = password, FirstName = "Amara", LastName = "Williams", Role = "Participant" };
        var david = new User { Email = "david@raceday.co.za", Password = password, FirstName = "David", LastName = "Mathews", Role = "Participant" };
        var lindiwe = new User { Email = "lindiwe@raceday.co.za", Password = password, FirstName = "Lindiwe", LastName = "Zulu", Role = "Participant" };
        db.Users.AddRange(thabo, priya, amara, david, lindiwe);
        db.SaveChanges();

        // Two open events and one completed event
        var capeTown = new Event
        {
            UserId = thabo.UserId, TypeId = run.TypeId, EventStatusId = open.EventStatusId,
            Name = "Cape Town Marathon 2027", Description = "A scenic marathon along the Atlantic Seaboard",
            EventDate = new DateTime(2027, 5, 15, 6, 0, 0), Location = "Cape Town Stadium, Cape Town", Distance = 42.20m
        };
        var durban = new Event
        {
            UserId = priya.UserId, TypeId = cycle.TypeId, EventStatusId = open.EventStatusId,
            Name = "Durban Cycle Challenge 2027", Description = "Fast and flat cycle race along the Durban beachfront",
            EventDate = new DateTime(2027, 6, 20, 7, 0, 0), Location = "Moses Mabhida Stadium, Durban", Distance = 95.00m
        };
        var pretoria = new Event
        {
            UserId = thabo.UserId, TypeId = run.TypeId, EventStatusId = completed.EventStatusId,
            Name = "Pretoria Night Run 2026", Description = "A night run through the lit up streets of Pretoria",
            EventDate = new DateTime(2026, 9, 5, 19, 0, 0), Location = "Union Buildings, Pretoria", Distance = 15.00m
        };
        db.Events.AddRange(capeTown, durban, pretoria);
        db.SaveChanges();

        // Categories for each event
        db.Categories.AddRange(
            new Category { EventId = capeTown.EventId, CategoryName = "Elite Women", AgeGroup = "18-34" },
            new Category { EventId = capeTown.EventId, CategoryName = "Elite Men", AgeGroup = "18-34" },
            new Category { EventId = capeTown.EventId, CategoryName = "Open Women", AgeGroup = "35-49" },
            new Category { EventId = capeTown.EventId, CategoryName = "Open Men", AgeGroup = "35-49" },
            new Category { EventId = durban.EventId, CategoryName = "Amateur Men", AgeGroup = "31-45" },
            new Category { EventId = durban.EventId, CategoryName = "Amateur Women", AgeGroup = "31-45" });
        var pretoriaMen = new Category { EventId = pretoria.EventId, CategoryName = "Elite Men", AgeGroup = "18-40" };
        var pretoriaWomen = new Category { EventId = pretoria.EventId, CategoryName = "Elite Women", AgeGroup = "18-40" };
        db.Categories.AddRange(pretoriaMen, pretoriaWomen);
        db.SaveChanges();

        // Three people entered the completed event
        var amaraEnrolment = new Enrolment { UserId = amara.UserId, EventId = pretoria.EventId, CategoryId = pretoriaWomen.CategoryId, StatusId = confirmed.StatusId };
        var davidEnrolment = new Enrolment { UserId = david.UserId, EventId = pretoria.EventId, CategoryId = pretoriaMen.CategoryId, StatusId = confirmed.StatusId };
        var lindiweEnrolment = new Enrolment { UserId = lindiwe.UserId, EventId = pretoria.EventId, CategoryId = pretoriaWomen.CategoryId };
        db.Enrolments.AddRange(amaraEnrolment, davidEnrolment, lindiweEnrolment);
        db.SaveChanges();

        // Only Amara and David have results, Lindiwe is left without one
        db.Results.AddRange(
            new Result { EnrolmentId = amaraEnrolment.EnrolmentId, FinishTime = new TimeSpan(2, 5, 40), FinishingPosition = 34 },
            new Result { EnrolmentId = davidEnrolment.EnrolmentId, FinishTime = new TimeSpan(2, 12, 55), FinishingPosition = 56 });
        db.SaveChanges();
    }
}
