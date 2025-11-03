using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProjectDefense.Shared.Data;
using ProjectDefense.Shared.Entities;

namespace ProjectDefense.Web.Pages.Prowadzacy.Rezerwacje
{
    [Authorize(Roles = "Prowadzący")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        
        [BindProperty(SupportsGet = true)]
        public bool PokazPrzeszle { get; set; }

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Rezerwacja> Rezerwacje { get; set; } = new();
        public List<Sala> Sale { get; set; } = new();
        
        [BindProperty(SupportsGet = true)]
        public int? FiltrowanaSalaId { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public string Status { get; set; } = "all";

        public async Task OnGetAsync()
        {
            Sale = await _context.Sale.Where(s => s.IsActive).ToListAsync();

            var query = _context.Rezerwacje
                .Include(r => r.DostepnoscProwadzacego)
                .ThenInclude(d => d.Sala)
                .Include(r => r.Student)
                .Where(r => r.IsActive)
                .AsQueryable();

            if (FiltrowanaSalaId.HasValue)
            {
                query = query.Where(r => r.DostepnoscProwadzacego.SalaId == FiltrowanaSalaId.Value);
            }

            if (Status == "zajete")
            {
                query = query.Where(r => r.StudentId != null);
            }
            else if (Status == "wolne")
            {
                query = query.Where(r => r.StudentId == null);
            }
            
            if (!PokazPrzeszle)
            {
                query = query.Where(r => r.CzasZakonczenia > DateTime.UtcNow);
            }

            Rezerwacje = await query
                .OrderBy(r => r.CzasRozpoczecia)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostCancelAsync(int id)
        {
            var rezerwacja = await _context.Rezerwacje.FindAsync(id);
            
            if (rezerwacja == null)
                return NotFound();

            rezerwacja.StudentId = null;
            rezerwacja.DataRezerwacji = null;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Rezerwacja została anulowana.";
            return RedirectToPage();
        }
        
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var rezerwacja = await _context.Rezerwacje.FindAsync(id);

            if (rezerwacja == null)
                return NotFound();

            _context.Rezerwacje.Remove(rezerwacja);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Slot został usunięty na stałe.";
            return RedirectToPage();
        }
    }
}
