using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProjectDefense.Shared.Data;
using ProjectDefense.Shared.Entities;

namespace ProjectDefense.Web.Pages.Prowadzacy.Sale
{
    [Authorize(Roles = "Prowadzący")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        
        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }
        
        public List<Sala> Sale { get; set; } = new();
        
        public async Task OnGetAsync()
        {
            Sale = await _context.Sale
                .Where(s => s.IsActive)
                .OrderBy(s => s.NumerSali)
                .ToListAsync();
        }
        
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var sala = await _context.Sale.FindAsync(id);
            
            if (sala == null)
                return NotFound();
            
            sala.IsActive = false;
            await _context.SaveChangesAsync();
            
            return RedirectToPage();
        }
    }
}