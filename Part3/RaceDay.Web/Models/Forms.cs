using System.ComponentModel.DataAnnotations;

namespace RaceDay.Web.Models;

// These classes are what the forms fill in

public class LoginForm
{
    [Required(ErrorMessage = "Please enter your email.")]
    [EmailAddress(ErrorMessage = "That does not look like an email address.")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Please enter your password.")]
    public string Password { get; set; } = "";
}

public class RegisterForm
{
    [Required(ErrorMessage = "Please enter your first name."), MaxLength(50)]
    [Display(Name = "First name")]
    public string FirstName { get; set; } = "";

    [Required(ErrorMessage = "Please enter your last name."), MaxLength(50)]
    [Display(Name = "Last name")]
    public string LastName { get; set; } = "";

    [Required(ErrorMessage = "Please enter your email."), MaxLength(100)]
    [EmailAddress(ErrorMessage = "That does not look like an email address.")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Please choose a password.")]
    [MinLength(6, ErrorMessage = "The password must be at least 6 characters.")]
    public string Password { get; set; } = "";

    [Required(ErrorMessage = "Please choose a role.")]
    [Display(Name = "I am a")]
    public string Role { get; set; } = "Participant";
}

public class ProfileForm
{
    [Required(ErrorMessage = "Please enter your first name."), MaxLength(50)]
    [Display(Name = "First name")]
    public string FirstName { get; set; } = "";

    [Required(ErrorMessage = "Please enter your last name."), MaxLength(50)]
    [Display(Name = "Last name")]
    public string LastName { get; set; } = "";

    [Required(ErrorMessage = "Please enter your email."), MaxLength(100)]
    [EmailAddress(ErrorMessage = "That does not look like an email address.")]
    public string Email { get; set; } = "";
}

public class EventForm
{
    public int EventId { get; set; }

    [Required(ErrorMessage = "Please enter the event name."), MaxLength(100)]
    public string Name { get; set; } = "";

    [Required(ErrorMessage = "Please add a description."), MaxLength(500)]
    public string Description { get; set; } = "";

    [Required(ErrorMessage = "Please choose a date and time.")]
    public DateTime? Date { get; set; }

    [Required(ErrorMessage = "Please enter the location."), MaxLength(200)]
    public string Location { get; set; } = "";

    [Range(0.01, 1000, ErrorMessage = "Distance must be between 0.01 and 1000 km.")]
    [Display(Name = "Distance (km)")]
    public decimal Distance { get; set; }

    [Range(1, 5, ErrorMessage = "Please choose an event type.")]
    [Display(Name = "Event type")]
    public int EventTypeId { get; set; } = 1;

    // Only used when editing
    [Display(Name = "Status")]
    public int EventStatusId { get; set; } = 2;
}

public class CategoryForm
{
    public int CategoryId { get; set; }
    public int EventId { get; set; }

    [Required(ErrorMessage = "Please enter a category name."), MaxLength(100)]
    [Display(Name = "Category name")]
    public string CategoryName { get; set; } = "";

    [MaxLength(50)]
    [Display(Name = "Age group (optional)")]
    public string? AgeGroup { get; set; }
}

public class ResultForm
{
    public int EventId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Please choose a participant.")]
    public int EnrolmentId { get; set; }

    // Typed as hh:mm:ss
    [Required(ErrorMessage = "Please enter the finish time like 02:05:40.")]
    [Display(Name = "Finish time")]
    public string FinishTime { get; set; } = "";

    [Range(1, 100000, ErrorMessage = "Position must be 1 or more.")]
    [Display(Name = "Finishing position")]
    public int FinishingPosition { get; set; }
}
