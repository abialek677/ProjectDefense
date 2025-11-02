using System.ComponentModel.DataAnnotations;

namespace ProjectDefense.Shared.Entities;

public class DostepnoscProwadzacego
{
    public int Id { get; set; }
        
    [Required]
    public string ProwadzacyId { get; set; } = string.Empty;
        
    [Required]
    public int SalaId { get; set; }
        
    [Required]
    public DateTime DataPoczatkowa { get; set; }
        
    [Required]
    public DateTime DataKoncowa { get; set; }
        
    [Required]
    public TimeSpan GodzinaRozpoczecia { get; set; }
        
    [Required]
    public TimeSpan GodzinaZakonczenia { get; set; }
        
    [Required]
    [Range(5, 120, ErrorMessage = "Czas trwania slotu musi być między 5 a 120 minutami")]
    public int CzasTrwaniaSlotuWMin { get; set; }
        
    public bool IsBlocked { get; set; } = false;
        
    // Navigation properties
    public virtual ApplicationUser Prowadzacy { get; set; }
    public virtual Sala Sala { get; set; }
    public virtual ICollection<Rezerwacja> Rezerwacje { get; set; }
}