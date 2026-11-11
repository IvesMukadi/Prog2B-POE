namespace RaceDay.Web.Models;

// The API has no endpoint for these lists, so they are written here (same ids as the database)
public static class Lookups
{
    public static readonly Dictionary<int, string> EventTypes = new()
    {
        { 1, "Run" }, { 2, "Walk" }, { 3, "Cycle" }, { 4, "Triathlon" }, { 5, "Ultra Marathon" }
    };

    public static readonly Dictionary<int, string> EventStatuses = new()
    {
        { 1, "Draft" }, { 2, "Open" }, { 3, "Closed" }, { 4, "Completed" }, { 5, "Cancelled" }
    };

    public static readonly Dictionary<int, string> EnrolmentStatuses = new()
    {
        { 1, "Pending" }, { 2, "Confirmed" }, { 3, "Cancelled" }, { 4, "Completed" }, { 5, "Waitlisted" }
    };

    // 1 becomes 1st, 2 becomes 2nd and so on
    public static string Ordinal(int number)
    {
        if (number % 100 >= 11 && number % 100 <= 13)
        {
            return number + "th";
        }

        return (number % 10) switch
        {
            1 => number + "st",
            2 => number + "nd",
            3 => number + "rd",
            _ => number + "th"
        };
    }
}
