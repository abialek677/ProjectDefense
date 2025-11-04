using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProjectDefense.Shared.Data;
using ProjectDefense.Shared.Entities;

namespace ProjectDefense.Web.Pages.Instructor.Blocks
{
    [Authorize(Roles = "Instructor")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<StudentBlock> Blocks { get; set; } = new();

        public async Task OnGetAsync()
        {
            Blocks = await _context.StudentBlocks
                .Include(b => b.Student)
                .Include(b => b.BlockingInstructor)
                .OrderByDescending(b => b.BlockDate)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostUnblockAsync(int id)
        {
            var block = await _context.StudentBlocks.FindAsync(id);
            
            if (block == null)
                return NotFound();

            block.IsActive = false;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Student has been unblocked.";
            return RedirectToPage();
        }
    }
}