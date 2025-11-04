using System.ComponentModel.DataAnnotations;

namespace ProjectDefense.Shared.Entities;

public class InstructorAvailability
{
    public int Id { get; set; }
    
    public string? InstructorId { get; set; }
        
    [Required]
    public int RoomId { get; set; }
        
    [Required]
    public DateTime StartDate { get; set; }
        
    [Required]
    public DateTime EndDate { get; set; }
        
    [Required]
    public TimeSpan StartHour { get; set; }
        
    [Required]
    public TimeSpan EndHour { get; set; }
        
    [Required]
    [Range(5, 120, ErrorMessage = "Czas trwania slotu musi być między 5 a 120 minutami")]
    public int SlotDurationMinutes { get; set; }
        
    public bool IsBlocked { get; set; } = false;
        
    // Navigation properties
    public virtual ApplicationUser? Instructor { get; set; }
    public virtual Room? Room { get; set; }
    public virtual ICollection<Reservation> Reservations { get; set; } = [];
}