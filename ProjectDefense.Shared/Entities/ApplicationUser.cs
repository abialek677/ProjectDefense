using Microsoft.AspNetCore.Identity;

namespace ProjectDefense.Shared.Entities;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
    // Navigation properties
    public virtual ICollection<Rezerwacja> Rezerwacje { get; set; }
    public virtual ICollection<DostepnoscProwadzacego> Dostepnosci { get; set; }
}