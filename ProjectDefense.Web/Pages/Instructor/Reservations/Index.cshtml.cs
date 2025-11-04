using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProjectDefense.Shared.Data;
using ProjectDefense.Shared.Entities;

namespace ProjectDefense.Web.Pages.Instructor.Reservations
{
    [Authorize(Roles = "Instructor")]
    public class Index : PageModel
    {
        private readonly ApplicationDbContext _context;

        public Index(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Reservation> Reservations { get; set; } = new();
        public List<Room> Rooms { get; set; } = new();

        [BindProperty(SupportsGet = true, Name = "roomId")]
        public int? FilteredRoomId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Status { get; set; } = "all";

        [BindProperty(SupportsGet = true)]
        public bool ShowPast { get; set; }

        public async Task OnGetAsync()
        {
            Rooms = await _context.Rooms
                .Where(r => r.IsActive)
                .OrderBy(r => r.RoomNumber)
                .ToListAsync();

            var query = _context.Reservations
                .Include(r => r.InstructorAvailability)
                .ThenInclude(a => a.Room)
                .Include(r => r.Student)
                .Where(r => r.IsActive)
                .AsQueryable();

            if (FilteredRoomId.HasValue)
                query = query.Where(r => r.InstructorAvailability.RoomId == FilteredRoomId.Value);

            if (Status == "booked")
                query = query.Where(r => r.StudentId != null);
            else if (Status == "free")
                query = query.Where(r => r.StudentId == null);

            if (!ShowPast)
                query = query.Where(r => r.EndTime > DateTime.UtcNow);

            Reservations = await query.OrderBy(r => r.StartTime).ToListAsync();
        }

        public async Task<IActionResult> OnPostCancelAsync(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
                return NotFound();

            reservation.StudentId = null;
            reservation.ReservationDate = null;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "The reservation has been canceled.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
                return NotFound();

            _context.Reservations.Remove(reservation);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "The slot was permanently deleted.";
            return RedirectToPage();
        }
    }
}
