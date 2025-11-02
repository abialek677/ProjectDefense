using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProjectDefense.Shared.Data;
using ProjectDefense.Shared.Entities;

namespace ProjectDefense.Web.Pages.Prowadzacy.Dostepnosci
{
    [Authorize(Roles = "Prowadzący")]
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
        public DostepnoscProwadzacego Dostepnosc { get; set; } = new();

        public SelectList SaleSelectList { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            await LoadSaleSelectList();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadSaleSelectList();
                return Page();
            }

            // Walidacja biznesowa
            if (Dostepnosc.DataKoncowa < Dostepnosc.DataPoczatkowa)
            {
                ModelState.AddModelError("", "Data końcowa musi być późniejsza niż data początkowa.");
                await LoadSaleSelectList();
                return Page();
            }

            if (Dostepnosc.GodzinaZakonczenia <= Dostepnosc.GodzinaRozpoczecia)
            {
                ModelState.AddModelError("", "Godzina zakończenia musi być późniejsza niż godzina rozpoczęcia.");
                await LoadSaleSelectList();
                return Page();
            }

            // Sprawdź konflikty
            var hasConflict = await _context.DostepnosciProwadzacych
                .AnyAsync(d => d.SalaId == Dostepnosc.SalaId &&
                              d.DataPoczatkowa <= Dostepnosc.DataKoncowa &&
                              d.DataKoncowa >= Dostepnosc.DataPoczatkowa &&
                              !d.IsBlocked);

            if (hasConflict)
            {
                ModelState.AddModelError("", "Istnieje konflikt z inną dostępnością dla tej sali w tym okresie.");
                await LoadSaleSelectList();
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);
            Dostepnosc.ProwadzacyId = user.Id;

            _context.DostepnosciProwadzacych.Add(Dostepnosc);
            await _context.SaveChangesAsync();

            // Generowanie slotów
            await GenerujSlotyAsync(Dostepnosc);

            TempData["SuccessMessage"] = "Dostępność została dodana i sloty wygenerowane.";
            return RedirectToPage("./Index");
        }

        private async Task GenerujSlotyAsync(DostepnoscProwadzacego dostepnosc)
        {
            var sloty = new List<Rezerwacja>();
            var currentDate = dostepnosc.DataPoczatkowa.Date;

            while (currentDate <= dostepnosc.DataKoncowa.Date)
            {
                var currentTime = dostepnosc.GodzinaRozpoczecia;
                
                while (currentTime.Add(TimeSpan.FromMinutes(dostepnosc.CzasTrwaniaSlotuWMin)) <= dostepnosc.GodzinaZakonczenia)
                {
                    var slot = new Rezerwacja
                    {
                        DostepnoscProwadzacegoId = dostepnosc.Id,
                        CzasRozpoczecia = currentDate.Add(currentTime),
                        CzasZakonczenia = currentDate.Add(currentTime).AddMinutes(dostepnosc.CzasTrwaniaSlotuWMin),
                        StudentId = null,
                        IsActive = true
                    };
                    sloty.Add(slot);
                    
                    currentTime = currentTime.Add(TimeSpan.FromMinutes(dostepnosc.CzasTrwaniaSlotuWMin));
                }
                
                currentDate = currentDate.AddDays(1);
            }

            _context.Rezerwacje.AddRange(sloty);
            await _context.SaveChangesAsync();
        }

        private async Task LoadSaleSelectList()
        {
            var sale = await _context.Sale
                .Where(s => s.IsActive)
                .OrderBy(s => s.NumerSali)
                .ToListAsync();
            
            SaleSelectList = new SelectList(sale, "Id", "Nazwa");
        }
    }
}
