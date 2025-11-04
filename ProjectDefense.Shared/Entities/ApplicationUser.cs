using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace ProjectDefense.Shared.Entities;

public class ApplicationUser : IdentityUser
{
    [Required(ErrorMessage = "First name is required")]
    public string FirstName { get; set; }
    
    [Required(ErrorMessage = "Last name is required")]
    public string LastName { get; set; }
    
    public string FullName => $"{FirstName} {LastName}";
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
    // Navigation properties
    public virtual ICollection<Reservation> Reservations { get; set; } = [];
    public virtual ICollection<InstructorAvailability> Availabilities { get; set; } = [];
}