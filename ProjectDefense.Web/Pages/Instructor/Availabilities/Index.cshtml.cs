using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProjectDefense.Shared.Data;
using ProjectDefense.Shared.Entities;

namespace ProjectDefense.Web.Pages.Instructor.Availabilities
{
    [Authorize(Roles = "Instructor")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public List<InstructorAvailability> Availabilities { get; set; } = new();

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            Availabilities = await _context.InstructorAvailabilities
                .Include(a => a.Room)
                .Where(a => a.InstructorId == user.Id)
                .OrderByDescending(a => a.StartDate)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostBlockAsync(int id)
        {
            var availability = await _context.InstructorAvailabilities.FindAsync(id);
            if (availability == null)
                return NotFound();

            availability.IsBlocked = true;

            // Cancel all reservations in this period
            var reservations = await _context.Reservations
                .Where(r => r.InstructorAvailabilityId == id && r.StudentId != null)
                .ToListAsync();

            foreach (var res in reservations)
            {
                res.StudentId = null;
                res.ReservationDate = null;
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Availability blocked and reservations canceled.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUnblockAsync(int id)
        {
            var availability = await _context.InstructorAvailabilities.FindAsync(id);
            if (availability == null)
                return NotFound();

            availability.IsBlocked = false;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Availability unblocked.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var availability = await _context.InstructorAvailabilities
                .Include(a => a.Reservations)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (availability == null)
                return NotFound();

            _context.Reservations.RemoveRange(availability.Reservations);
            _context.InstructorAvailabilities.Remove(availability);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Availability deleted.";
            return RedirectToPage();
        }
    }
}
