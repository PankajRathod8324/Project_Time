using System.ComponentModel.DataAnnotations;

namespace Entities.ViewModel;

public class WaitingListVM
{
    public int WaitingListId { get; set; }

    public DateTime WaitingTime { get; set; }

    // public TimeSpan WaitingTime { get; set; }
    public TimeSpan Duration { get; set; } 
    public int? SectionId { get; set; }

    public string SectionName {get; set;}

    [Required(ErrorMessage = "Name is required")]
    public string Name { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    [Required(ErrorMessage = "Phone number is required")]
    [RegularExpression(@"^\d{10}$", ErrorMessage = "Enter a valid 10-digit phone number")]
    public string Phone { get; set; } = null!;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Enter a Valid Email")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Number of persons is required")]
    [Range(1, 20, ErrorMessage = "Number of persons must be between 1 and 20")]
    public int NoOfPerson { get; set; }

    public bool IsDeleted { get; set; }

    public int CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public int ModifiedBy { get; set; }
    public DateTime ModifiedAt { get; set; }
}