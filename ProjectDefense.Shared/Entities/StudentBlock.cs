using System.ComponentModel.DataAnnotations;

namespace ProjectDefense.Shared.Entities;

public class StudentBlock
{
    public int Id { get; set; }
        
    [Required]
    public string StudentId { get; set; } = string.Empty;
        
    [Required]
    public string BlockReason { get; set; } = string.Empty;
        
    public DateTime BlockDate { get; set; } = DateTime.UtcNow;
        
    public string BlockingInstructorId { get; set; } = string.Empty;
        
    public bool IsActive { get; set; } = true;
        
    // Navigation properties
    public virtual ApplicationUser Student { get; set; }
    public virtual ApplicationUser BlockingInstructor { get; set; }
}