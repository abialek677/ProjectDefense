using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProjectDefense.Shared.Data;
using ProjectDefense.Shared.Entities;

namespace ProjectDefense.Web.Pages.Student.Rezerwacje
{
    [Authorize(Roles = "Student")]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CreateModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public int SalaId { get; set; }
        [BindProperty]
        public DateTime Date { get; set; }
        [BindProperty]
        public int SlotId { get; set; }
        public SelectList SaleSelectList { get; set; }
        public SelectList SlotSelectList { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            SaleSelectList = new SelectList(
                await _context.Sale.Where(s => s.IsActive).OrderBy(s => s.Nazwa).ToListAsync(), 
                "Id", "Nazwa"
            );
            // Początkowo SlotSelectList może być pusty.
            SlotSelectList = new SelectList(new List<SelectListItem>(), "Value", "Text");
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            SaleSelectList = new SelectList(
                await _context.Sale.Where(s => s.IsActive).OrderBy(s => s.Nazwa).ToListAsync(), 
                "Id", "Nazwa"
            );
            
            if (!ModelState.IsValid)
            {
                SlotSelectList = new SelectList(new List<SelectListItem>(), "Value", "Text");
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);
            
            var isBanned = await _context.BlokadyStudentow
                .AnyAsync(b => b.StudentId == user.Id && b.IsActive);

            if (isBanned)
            {
                ModelState.AddModelError(string.Empty, "Twoje konto zostało zablokowane przez prowadzącego. Skontaktuj się z administracją.");
                return Page();
            }

            var slot = await _context.Rezerwacje
                .Include(r => r.DostepnoscProwadzacego)
                .FirstOrDefaultAsync(r =>
                    r.Id == SlotId &&
                    r.DostepnoscProwadzacego.SalaId == SalaId &&
                    r.CzasRozpoczecia.Date == Date.Date &&
                    r.IsActive &&
                    r.StudentId == null);

            if (slot == null)
            {
                TempData["ErrorMessage"] = "Wybrany slot jest już niedostępny. Wybierz inny.";
                SlotSelectList = new SelectList(new List<SelectListItem>(), "Value", "Text");
                return Page();
            }

            // Sprawdź czy student nie ma już rezerwacji
            var alreadyReserved = await _context.Rezerwacje
                .AnyAsync(r => r.StudentId == user.Id && r.IsActive);
            if (alreadyReserved)
            {
                TempData["ErrorMessage"] = "Masz już aktywną rezerwację. Najpierw musisz ją anulować.";
                SlotSelectList = new SelectList(new List<SelectListItem>(), "Value", "Text");
                return Page();
            }

            slot.StudentId = user.Id;
            slot.DataRezerwacji = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Rezerwacja została utworzona!";
            return RedirectToPage("Index");
        }

        // Opcjonalnie: AJAX/partial do dynamicznego ładowania dostępnych slotów!
    }
}
