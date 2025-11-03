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
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        
        public IndexModel(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        
        public List<Rezerwacja> WolneSloty { get; set; } = new();
        public Rezerwacja? MojaRezerwacja { get; set; }
        
        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            var now = DateTime.UtcNow;

            var isBanned = await _context.BlokadyStudentow
                .AnyAsync(b => b.StudentId == user.Id && b.IsActive);

            if (isBanned)
            {
                TempData["ErrorMessage"] = "Twoje konto zostało zablokowane przez prowadzącego. Skontaktuj się z administracją.";
                WolneSloty = new List<Rezerwacja>();
                MojaRezerwacja = null;
                return;
            }
            
            // Pobierz rezerwację studenta
            MojaRezerwacja = await _context.Rezerwacje
                .Include(r => r.DostepnoscProwadzacego)
                .ThenInclude(d => d.Sala)
                .Include(r => r.DostepnoscProwadzacego)
                .ThenInclude(d => d.Prowadzacy)
                .FirstOrDefaultAsync(r => r.StudentId == user.Id && 
                                         r.IsActive && 
                                         r.CzasRozpoczecia > now);
            
            // Jeśli student nie ma rezerwacji, pokaż dostępne sloty
            if (MojaRezerwacja == null)
            {
                WolneSloty = await _context.Rezerwacje
                    .Include(r => r.DostepnoscProwadzacego)
                    .ThenInclude(d => d.Sala)
                    .Include(r => r.DostepnoscProwadzacego)
                    .ThenInclude(d => d.Prowadzacy)
                    .Where(r => r.StudentId == null && 
                               r.IsActive && 
                               r.CzasRozpoczecia > now &&
                               !r.DostepnoscProwadzacego.IsBlocked)
                    .OrderBy(r => r.CzasRozpoczecia)
                    .Take(20)
                    .ToListAsync();
            }
        }
        
        public async Task<IActionResult> OnPostBookAsync(int slotId)
        {
            var user = await _userManager.GetUserAsync(User);
            
            // Sprawdź czy student nie jest zbanowany
            var isBanned = await _context.BlokadyStudentow
                .AnyAsync(b => b.StudentId == user.Id && b.IsActive);
            
            if (isBanned)
            {
                ModelState.AddModelError("", "Twoje konto jest zablokowane");
                return Page();
            }
            
            // Sprawdź czy student już ma rezerwację
            var hasReservation = await _context.Rezerwacje
                .AnyAsync(r => r.StudentId == user.Id && 
                              r.IsActive && 
                              r.CzasRozpoczecia > DateTime.UtcNow);
            
            if (hasReservation)
            {
                ModelState.AddModelError("", "Masz już aktywną rezerwację");
                return Page();
            }
            
            var slot = await _context.Rezerwacje
                .Include(r => r.DostepnoscProwadzacego)
                .FirstOrDefaultAsync(r => r.Id == slotId);
            
            if (slot == null || slot.StudentId != null)
            {
                ModelState.AddModelError("", "Ten slot jest już zajęty");
                return Page();
            }
            
            if (slot.DostepnoscProwadzacego.IsBlocked)
            {
                ModelState.AddModelError("", "Ten termin jest zablokowany");
                return Page();
            }
            
            slot.StudentId = user.Id;
            slot.DataRezerwacji = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            
            return RedirectToPage();
        }
        
        public async Task<IActionResult> OnPostCancelAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            
            var reservation = await _context.Rezerwacje
                .FirstOrDefaultAsync(r => r.StudentId == user.Id && 
                                         r.IsActive && 
                                         r.CzasRozpoczecia > DateTime.UtcNow);
            
            if (reservation == null)
                return NotFound();
            
            reservation.StudentId = null;
            reservation.DataRezerwacji = null;
            
            await _context.SaveChangesAsync();
            
            return RedirectToPage();
        }
    }
}
