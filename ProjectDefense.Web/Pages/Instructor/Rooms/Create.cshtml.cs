using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjectDefense.Shared.Data;
using ProjectDefense.Shared.Entities;

namespace ProjectDefense.Web.Pages.Instructor.Rooms
{
    [Authorize(Roles = "Instructor")]
    public class Create : PageModel
    {
        private readonly ApplicationDbContext _context;

        public Create(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Room Room { get; set; } = new();

        public IActionResult OnGet() => Page();

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            _context.Rooms.Add(Room);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Room added successfully.";
            return RedirectToPage("./Index");
        }
    }
}