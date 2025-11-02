using System.ComponentModel.DataAnnotations;

namespace ProjectDefense.Shared.Entities;

public class BlokadaStudenta
{
    public int Id { get; set; }
        
    [Required]
    public string StudentId { get; set; } = string.Empty;
        
    [Required]
    public string Powod { get; set; } = string.Empty;
        
    public DateTime DataBlokady { get; set; } = DateTime.UtcNow;
        
    public string BlokowalProwadzacyId { get; set; } = string.Empty;
        
    public bool IsActive { get; set; } = true;
        
    // Navigation properties
    public virtual ApplicationUser Student { get; set; }
    public virtual ApplicationUser BlokowalProwadzacy { get; set; }
}