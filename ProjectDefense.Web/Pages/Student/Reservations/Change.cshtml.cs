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
    public class ChangeModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ChangeModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public List<Reservation> AvailableSlots { get; set; } = new();
        public Reservation CurrentReservation { get; set; }

        [BindProperty]
        public int NewSlotId { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            CurrentReservation = await _context.Reservations
                .Include(r => r.InstructorAvailability)
                    .ThenInclude(d => d.Room)
                .FirstOrDefaultAsync(r => r.StudentId == user.Id && r.IsActive);

            if (CurrentReservation == null)
            {
                TempData["ErrorMessage"] = "You don't have an active reservation to change.";
                return RedirectToPage("Index");
            }

            AvailableSlots = await _context.Reservations
                .Include(r => r.InstructorAvailability)
                    .ThenInclude(d => d.Instructor)
                .Where(r =>
                    r.InstructorAvailability.RoomId == CurrentReservation.InstructorAvailability.RoomId &&
                    r.StartTime.Date == CurrentReservation.StartTime.Date &&
                    r.IsActive && r.StudentId == null)
                .OrderBy(r => r.StartTime)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            var currentReservation = await _context.Reservations
                .FirstOrDefaultAsync(r => r.StudentId == user.Id && r.IsActive);

            if (currentReservation == null)
            {
                TempData["ErrorMessage"] = "You don't have an active reservation.";
                return RedirectToPage("Index");
            }

            var newSlot = await _context.Reservations
                .FirstOrDefaultAsync(r => r.Id == NewSlotId && r.IsActive && r.StudentId == null);

            if (newSlot == null)
            {
                TempData["ErrorMessage"] = "The selected slot is already taken!";
                return RedirectToPage();
            }
            
            currentReservation.StudentId = null;
            currentReservation.ReservationDate = null;
            
            newSlot.StudentId = user.Id;
            newSlot.ReservationDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Reservation time has been changed!";
            return RedirectToPage("Index");
        }
    }
}
