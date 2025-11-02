using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProjectDefense.Shared.Data;
using ProjectDefense.Shared.Entities;

namespace ProjectDefense.Web.Pages.Student.Rezerwacje
{
    [Authorize(Roles = "Student")]
    public class ChangeModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ChangeModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public Rezerwacja AktualnaRezerwacja { get; set; }
        public List<Rezerwacja> WolneSloty { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            var now = DateTime.UtcNow;

            AktualnaRezerwacja = await _context.Rezerwacje
                .Include(r => r.DostepnoscProwadzacego)
                .ThenInclude(d => d.Sala)
                .Include(r => r.DostepnoscProwadzacego)
                .ThenInclude(d => d.Prowadzacy)
                .FirstOrDefaultAsync(r => r.StudentId == user.Id && 
                                         r.IsActive && 
                                         r.CzasRozpoczecia > now);

            if (AktualnaRezerwacja == null)
            {
                TempData["ErrorMessage"] = "Nie masz aktywnej rezerwacji do zmiany.";
                return RedirectToPage("./Index");
            }

            WolneSloty = await _context.Rezerwacje
                .Include(r => r.DostepnoscProwadzacego)
                .ThenInclude(d => d.Sala)
                .Include(r => r.DostepnoscProwadzacego)
                .ThenInclude(d => d.Prowadzacy)
                .Where(r => r.StudentId == null && 
                           r.IsActive && 
                           r.CzasRozpoczecia > now &&
                           !r.DostepnoscProwadzacego.IsBlocked &&
                           r.Id != AktualnaRezerwacja.Id)
                .OrderBy(r => r.CzasRozpoczecia)
                .Take(30)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int nowySlotId)
        {
            var user = await _userManager.GetUserAsync(User);
            var now = DateTime.UtcNow;

            var staraRezerwacja = await _context.Rezerwacje
                .FirstOrDefaultAsync(r => r.StudentId == user.Id && 
                                         r.IsActive && 
                                         r.CzasRozpoczecia > now);

            var nowaRezerwacja = await _context.Rezerwacje
                .Include(r => r.DostepnoscProwadzacego)
                .FirstOrDefaultAsync(r => r.Id == nowySlotId);

            if (staraRezerwacja == null || nowaRezerwacja == null)
            {
                TempData["ErrorMessage"] = "Nie można zmienić rezerwacji.";
                return RedirectToPage("./Index");
            }

            if (nowaRezerwacja.StudentId != null)
            {
                TempData["ErrorMessage"] = "Wybrany termin jest już zajęty.";
                return RedirectToPage();
            }

            if (nowaRezerwacja.DostepnoscProwadzacego.IsBlocked)
            {
                TempData["ErrorMessage"] = "Wybrany termin jest zablokowany.";
                return RedirectToPage();
            }

            // Zmień rezerwację
            staraRezerwacja.StudentId = null;
            staraRezerwacja.DataRezerwacji = null;

            nowaRezerwacja.StudentId = user.Id;
            nowaRezerwacja.DataRezerwacji = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Twoja rezerwacja została zmieniona pomyślnie.";
            return RedirectToPage("./Index");
        }
    }
}
