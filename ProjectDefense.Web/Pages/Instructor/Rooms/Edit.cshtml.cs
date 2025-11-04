using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProjectDefense.Shared.Data;
using ProjectDefense.Shared.Entities;

namespace ProjectDefense.Web.Pages.Instructor.Rooms
{
    [Authorize(Roles = "Instructor")]
    public class Edit : PageModel
    {
        private readonly ApplicationDbContext _context;

        public Edit(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Room Room { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id is null)
                return NotFound();

            Room = await _context.Rooms.FirstOrDefaultAsync(r => r.Id == id);

            return Room is null ? NotFound() : Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            _context.Attach(Room).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RoomExists(Room.Id))
                    return NotFound();
                throw;
            }

            TempData["SuccessMessage"] = "Room updated successfully.";
            return RedirectToPage("./Index");
        }

        private bool RoomExists(int id) =>
            _context.Rooms.Any(r => r.Id == id);
    }
}