using System.ComponentModel.DataAnnotations;

namespace ProjectDefense.Shared.Entities;

public class Room
{
    public int Id { get; set; }
        
    [Required(ErrorMessage = "Room name is required")]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
        
    [Required(ErrorMessage = "Room number is required")]
    [StringLength(20)]
    public string RoomNumber { get; set; } = string.Empty;
        
    public bool IsActive { get; set; } = true;
        
    // Navigation properties
    public virtual ICollection<InstructorAvailability> Availabilities { get; set; } = [];
}