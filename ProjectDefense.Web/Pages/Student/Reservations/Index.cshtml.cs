using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProjectDefense.Shared.Data;
using ProjectDefense.Shared.Entities;

namespace ProjectDefense.Web.Pages.Student.Reservations
{
    [Authorize(Roles = "Student")]
    public class Index : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public Index(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public List<Reservation> FreeReservations { get; set; } = new();
        public Reservation? MyReservation { get; set; }

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            var now = DateTime.UtcNow;

            var isBanned = await _context.StudentBlocks
                .AnyAsync(b => b.StudentId == user.Id && b.IsActive);

            if (isBanned)
            {
                TempData["ErrorMessage"] = "Your account has been blocked by the instructor. Please contact administration.";
                FreeReservations = new List<Reservation>();
                MyReservation = null;
                return;
            }

            // Load current student reservation
            MyReservation = await _context.Reservations
                .Include(r => r.InstructorAvailability)
                    .ThenInclude(a => a.Room)
                .Include(r => r.InstructorAvailability)
                    .ThenInclude(a => a.Instructor)
                .FirstOrDefaultAsync(r =>
                    r.StudentId == user.Id &&
                    r.IsActive &&
                    r.StartTime > now);

            // If no active reservation, show available slots
            if (MyReservation == null)
            {
                FreeReservations = await _context.Reservations
                    .Include(r => r.InstructorAvailability)
                        .ThenInclude(a => a.Room)
                    .Include(r => r.InstructorAvailability)
                        .ThenInclude(a => a.Instructor)
                    .Where(r =>
                        r.StudentId == null &&
                        r.IsActive &&
                        r.StartTime > now &&
                        !r.InstructorAvailability.IsBlocked)
                    .OrderBy(r => r.StartTime)
                    .Take(20)
                    .ToListAsync();
            }
        }

        public async Task<IActionResult> OnPostBookAsync(int slotId)
        {
            var user = await _userManager.GetUserAsync(User);

            // Check if the student is blocked
            var isBanned = await _context.StudentBlocks
                .AnyAsync(b => b.StudentId == user.Id && b.IsActive);

            if (isBanned)
            {
                ModelState.AddModelError("", "Your account is blocked.");
                return Page();
            }

            // Check if the student already has a reservation
            var hasReservation = await _context.Reservations
                .AnyAsync(r =>
                    r.StudentId == user.Id &&
                    r.IsActive &&
                    r.StartTime > DateTime.UtcNow);

            if (hasReservation)
            {
                ModelState.AddModelError("", "You already have an active reservation.");
                return Page();
            }

            var slot = await _context.Reservations
                .Include(r => r.InstructorAvailability)
                .FirstOrDefaultAsync(r => r.Id == slotId);

            if (slot == null || slot.StudentId != null)
            {
                ModelState.AddModelError("", "This slot is already taken.");
                return Page();
            }

            if (slot.InstructorAvailability.IsBlocked)
            {
                ModelState.AddModelError("", "This time slot is blocked.");
                return Page();
            }

            slot.StudentId = user.Id;
            slot.ReservationDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Reservation created successfully.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCancelAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            var reservation = await _context.Reservations
                .FirstOrDefaultAsync(r =>
                    r.StudentId == user.Id &&
                    r.IsActive &&
                    r.StartTime > DateTime.UtcNow);

            if (reservation == null)
                return NotFound();

            reservation.StudentId = null;
            reservation.ReservationDate = null;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Reservation cancelled successfully.";
            return RedirectToPage();
        }
    }
}
