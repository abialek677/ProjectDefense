using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProjectDefense.Shared.Data;
using ProjectDefense.Shared.Entities;

namespace ProjectDefense.Web.Pages.Instructor.Reservations
{
    [Authorize(Roles = "Instructor")]
    public class Reschedule : PageModel
    {
        private readonly ApplicationDbContext _context;

        public Reschedule(ApplicationDbContext context)
        {
            _context = context;
        }

        public Reservation CurrentReservation { get; set; } = null!;
        public List<Reservation> AvailableSlots { get; set; } = new();

        [BindProperty]
        public int ReservationId { get; set; }

        [BindProperty]
        public int NewSlotId { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            CurrentReservation = await _context.Reservations
                .Include(r => r.Student)
                .Include(r => r.InstructorAvailability)
                .ThenInclude(a => a.Room)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (CurrentReservation == null || CurrentReservation.StudentId == null)
                return NotFound();

            ReservationId = id;

            // Fetch available (free) slots
            var now = DateTime.UtcNow;
            AvailableSlots = await _context.Reservations
                .Include(r => r.InstructorAvailability)
                .ThenInclude(a => a.Room)
                .Where(r => r.StudentId == null &&
                            r.IsActive &&
                            r.StartTime > now &&
                            !r.InstructorAvailability.IsBlocked)
                .OrderBy(r => r.StartTime)
                .Take(50)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var oldReservation = await _context.Reservations.FindAsync(ReservationId);
            var newReservation = await _context.Reservations.FindAsync(NewSlotId);

            if (oldReservation == null || newReservation == null)
                return NotFound();

            if (newReservation.StudentId != null)
            {
                ModelState.AddModelError(string.Empty, "The selected slot is already booked.");
                return Page();
            }

            // Transfer student
            var studentId = oldReservation.StudentId;
            oldReservation.StudentId = null;
            oldReservation.ReservationDate = null;

            newReservation.StudentId = studentId;
            newReservation.ReservationDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "The student has been successfully rescheduled.";
            return RedirectToPage("./Index");
        }
    }
}
