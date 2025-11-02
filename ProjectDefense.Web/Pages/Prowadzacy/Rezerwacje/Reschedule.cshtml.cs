using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProjectDefense.Shared.Data;
using ProjectDefense.Shared.Entities;

namespace ProjectDefense.Web.Pages.Prowadzacy.Rezerwacje
{
    [Authorize(Roles = "Prowadzący")]
    public class RescheduleModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public RescheduleModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public Rezerwacja AktualnaRezerwacja { get; set; }
        public List<Rezerwacja> WolneSloty { get; set; } = new();

        [BindProperty]
        public int RezerwacjaId { get; set; }

        [BindProperty]
        public int NowySlotId { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            AktualnaRezerwacja = await _context.Rezerwacje
                .Include(r => r.Student)
                .Include(r => r.DostepnoscProwadzacego)
                .ThenInclude(d => d.Sala)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (AktualnaRezerwacja == null || AktualnaRezerwacja.StudentId == null)
                return NotFound();

            RezerwacjaId = id;

            // Pobierz wolne sloty
            var now = DateTime.UtcNow;
            WolneSloty = await _context.Rezerwacje
                .Include(r => r.DostepnoscProwadzacego)
                .ThenInclude(d => d.Sala)
                .Where(r => r.StudentId == null && 
                           r.IsActive && 
                           r.CzasRozpoczecia > now &&
                           !r.DostepnoscProwadzacego.IsBlocked)
                .OrderBy(r => r.CzasRozpoczecia)
                .Take(50)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var staraRezerwacja = await _context.Rezerwacje.FindAsync(RezerwacjaId);
            var nowaRezerwacja = await _context.Rezerwacje.FindAsync(NowySlotId);

            if (staraRezerwacja == null || nowaRezerwacja == null)
                return NotFound();

            if (nowaRezerwacja.StudentId != null)
            {
                ModelState.AddModelError("", "Wybrany slot jest już zajęty.");
                return Page();
            }

            // Przepisz studenta
            var studentId = staraRezerwacja.StudentId;
            staraRezerwacja.StudentId = null;
            staraRezerwacja.DataRezerwacji = null;

            nowaRezerwacja.StudentId = studentId;
            nowaRezerwacja.DataRezerwacji = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Student został przepisany na nowy termin.";
            return RedirectToPage("./Index");
        }
    }
}
