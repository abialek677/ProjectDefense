using System.ComponentModel.DataAnnotations;

namespace ProjectDefense.Shared.Entities;

public class Sala
{
    public int Id { get; set; }
        
    [Required(ErrorMessage = "Nazwa sali jest wymagana")]
    [StringLength(100)]
    public string Nazwa { get; set; } = string.Empty;
        
    [Required(ErrorMessage = "Numer sali jest wymagany")]
    [StringLength(20)]
    public string NumerSali { get; set; } = string.Empty;
        
    public bool IsActive { get; set; } = true;
        
    // Navigation properties
    public virtual ICollection<DostepnoscProwadzacego> Dostepnosci { get; set; } = [];
}