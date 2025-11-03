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

        public List<Rezerwacja> WolneSloty { get; set; } = new();
        public Rezerwacja CurrentReservation { get; set; }

        [BindProperty]
        public int NewSlotId { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            CurrentReservation = await _context.Rezerwacje
                .Include(r => r.DostepnoscProwadzacego)
                    .ThenInclude(d => d.Sala)
                .FirstOrDefaultAsync(r => r.StudentId == user.Id && r.IsActive);

            if (CurrentReservation == null)
            {
                TempData["ErrorMessage"] = "Nie masz aktywnej rezerwacji do zmiany.";
                return RedirectToPage("Index");
            }

            WolneSloty = await _context.Rezerwacje
                .Include(r => r.DostepnoscProwadzacego)
                    .ThenInclude(d => d.Prowadzacy)
                .Where(r =>
                    r.DostepnoscProwadzacego.SalaId == CurrentReservation.DostepnoscProwadzacego.SalaId &&
                    r.CzasRozpoczecia.Date == CurrentReservation.CzasRozpoczecia.Date &&
                    r.IsActive && r.StudentId == null)
                .OrderBy(r => r.CzasRozpoczecia)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            var currentReservation = await _context.Rezerwacje
                .FirstOrDefaultAsync(r => r.StudentId == user.Id && r.IsActive);

            if (currentReservation == null)
            {
                TempData["ErrorMessage"] = "Nie masz aktywnej rezerwacji.";
                return RedirectToPage("Index");
            }

            var newSlot = await _context.Rezerwacje
                .FirstOrDefaultAsync(r => r.Id == NewSlotId && r.IsActive && r.StudentId == null);

            if (newSlot == null)
            {
                TempData["ErrorMessage"] = "Wybrany slot jest już zajęty!";
                return RedirectToPage();
            }

            // Odpięcie starego slotu
            currentReservation.StudentId = null;
            currentReservation.DataRezerwacji = null;

            // Przypięcie nowego slotu
            newSlot.StudentId = user.Id;
            newSlot.DataRezerwacji = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Termin został zmieniony!";
            return RedirectToPage("Index");
        }
    }
}
