using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProjectDefense.Shared.Data;
using ProjectDefense.Shared.Entities;
using System.ComponentModel.DataAnnotations;

namespace ProjectDefense.Web.Pages.Prowadzacy.Blokady
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
        [Required(ErrorMessage = "Wybierz studenta")]
        public string StudentId { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Podaj powód blokady")]
        [StringLength(500, ErrorMessage = "Powód nie może przekraczać 500 znaków")]
        public string Powod { get; set; }

        public SelectList StudenciSelectList { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            await LoadStudenciSelectList();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadStudenciSelectList();
                return Page();
            }

            // Sprawdź czy student już jest zbanowany
            var isJuzZbanowany = await _context.BlokadyStudentow
                .AnyAsync(b => b.StudentId == StudentId && b.IsActive);

            if (isJuzZbanowany)
            {
                ModelState.AddModelError("", "Ten student jest już zbanowany.");
                await LoadStudenciSelectList();
                return Page();
            }

            var prowadzacy = await _userManager.GetUserAsync(User);

            var blokada = new BlokadaStudenta
            {
                StudentId = StudentId,
                Powod = Powod,
                BlokowalProwadzacyId = prowadzacy.Id,
                DataBlokady = DateTime.UtcNow,
                IsActive = true
            };

            _context.BlokadyStudentow.Add(blokada);

            // Anuluj wszystkie rezerwacje studenta
            var rezerwacje = await _context.Rezerwacje
                .Where(r => r.StudentId == StudentId && r.IsActive && r.CzasRozpoczecia > DateTime.UtcNow)
                .ToListAsync();

            foreach (var rez in rezerwacje)
            {
                rez.StudentId = null;
                rez.DataRezerwacji = null;
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Student został zbanowany, a jego rezerwacje anulowane.";
            return RedirectToPage("./Index");
        }

        private async Task LoadStudenciSelectList()
        {
            var studenci = await _userManager.GetUsersInRoleAsync("Student");
            StudenciSelectList = new SelectList(
                studenci.Select(s => new { Id = s.Id, Name = $"{s.FirstName} {s.LastName} ({s.Email})" }),
                "Id", "Name");
        }
    }
}
