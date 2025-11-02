using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProjectDefense.Shared.Data;
using ProjectDefense.Shared.Entities;

namespace ProjectDefense.Web.Pages.Prowadzacy.Blokady
{
    [Authorize(Roles = "Prowadzący")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<BlokadaStudenta> Blokady { get; set; } = new();

        public async Task OnGetAsync()
        {
            Blokady = await _context.BlokadyStudentow
                .Include(b => b.Student)
                .Include(b => b.BlokowalProwadzacy)
                .OrderByDescending(b => b.DataBlokady)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostUnblockAsync(int id)
        {
            var blokada = await _context.BlokadyStudentow.FindAsync(id);
            
            if (blokada == null)
                return NotFound();

            blokada.IsActive = false;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Student został odblokowany.";
            return RedirectToPage();
        }
    }
}