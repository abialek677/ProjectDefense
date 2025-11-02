using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProjectDefense.Shared.Data;
using ProjectDefense.Shared.Entities;

namespace ProjectDefense.Web.Pages.Prowadzacy.Dostepnosci
{
    [Authorize(Roles = "Prowadzący")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public List<DostepnoscProwadzacego> Dostepnosci { get; set; } = new();

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            Dostepnosci = await _context.DostepnosciProwadzacych
                .Include(d => d.Sala)
                .Where(d => d.ProwadzacyId == user.Id)
                .OrderByDescending(d => d.DataPoczatkowa)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostBlockAsync(int id)
        {
            var dostepnosc = await _context.DostepnosciProwadzacych.FindAsync(id);
            if (dostepnosc == null)
                return NotFound();

            dostepnosc.IsBlocked = true;
            
            // Anuluj wszystkie rezerwacje w tym okresie
            var rezerwacje = await _context.Rezerwacje
                .Where(r => r.DostepnoscProwadzacegoId == id && r.StudentId != null)
                .ToListAsync();
            
            foreach (var rez in rezerwacje)
            {
                rez.StudentId = null;
                rez.DataRezerwacji = null;
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Okres został zablokowany i rezerwacje anulowane.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUnblockAsync(int id)
        {
            var dostepnosc = await _context.DostepnosciProwadzacych.FindAsync(id);
            if (dostepnosc == null)
                return NotFound();

            dostepnosc.IsBlocked = false;
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = "Okres został odblokowany.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var dostepnosc = await _context.DostepnosciProwadzacych
                .Include(d => d.Rezerwacje)
                .FirstOrDefaultAsync(d => d.Id == id);
            
            if (dostepnosc == null)
                return NotFound();

            _context.Rezerwacje.RemoveRange(dostepnosc.Rezerwacje);
            _context.DostepnosciProwadzacych.Remove(dostepnosc);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Dostępność została usunięta.";
            return RedirectToPage();
        }
    }
}
