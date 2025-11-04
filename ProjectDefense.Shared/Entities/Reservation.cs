using System.ComponentModel.DataAnnotations;

namespace ProjectDefense.Shared.Entities;

public class Reservation
{
    public int Id { get; set; }
        
    [Required]
    public int InstructorAvailabilityId { get; set; }
        
    [Required]
    public DateTime StartTime { get; set; }
        
    [Required]
    public DateTime EndTime { get; set; }
    
    public string? StudentId { get; set; }
        
    public DateTime? ReservationDate { get; set; }
        
    public bool IsActive { get; set; } = true;
        
    // Navigation properties
    public virtual InstructorAvailability InstructorAvailability { get; set; }
    public virtual ApplicationUser? Student { get; set; }
}