using System.ComponentModel.DataAnnotations;

namespace ProjectDefense.Shared.Entities;

public class Rezerwacja
{
    public int Id { get; set; }
        
    [Required]
    public int DostepnoscProwadzacegoId { get; set; }
        
    [Required]
    public DateTime CzasRozpoczecia { get; set; }
        
    [Required]
    public DateTime CzasZakonczenia { get; set; }
        
    // Nullable - slot może być wolny
    public string? StudentId { get; set; }
        
    public DateTime? DataRezerwacji { get; set; }
        
    public bool IsActive { get; set; } = true;
        
    // Navigation properties
    public virtual DostepnoscProwadzacego DostepnoscProwadzacego { get; set; }
    public virtual ApplicationUser? Student { get; set; }
}