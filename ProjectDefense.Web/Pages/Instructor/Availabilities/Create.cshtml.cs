using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProjectDefense.Shared.Data;
using ProjectDefense.Shared.Entities;

namespace ProjectDefense.Web.Pages.Instructor.Availabilities
{
    [Authorize(Roles = "Instructor")]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CreateModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public InstructorAvailability Availability { get; set; } = new();

        public SelectList RoomSelectList { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            await LoadRoomSelectListAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadRoomSelectListAsync();
                return Page();
            }

            Availability.StartDate = DateTime.SpecifyKind(Availability.StartDate, DateTimeKind.Utc);
            Availability.EndDate = DateTime.SpecifyKind(Availability.EndDate, DateTimeKind.Utc);

            // Business validation
            if (Availability.EndDate < Availability.StartDate)
            {
                ModelState.AddModelError("", "End date must be later than start date.");
                await LoadRoomSelectListAsync();
                return Page();
            }

            if (Availability.EndHour <= Availability.StartHour)
            {
                ModelState.AddModelError("", "End time must be later than start time.");
                await LoadRoomSelectListAsync();
                return Page();
            }

            // Check conflicts
            var conflictExists = await _context.InstructorAvailabilities
                .AnyAsync(d => d.RoomId == Availability.RoomId &&
                               d.StartDate <= Availability.EndDate &&
                               d.EndDate >= Availability.StartDate &&
                               !d.IsBlocked);

            if (conflictExists)
            {
                ModelState.AddModelError("", "There is a conflicting availability for this room in the selected period.");
                await LoadRoomSelectListAsync();
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);
            Availability.InstructorId = user.Id;

            _context.InstructorAvailabilities.Add(Availability);
            await _context.SaveChangesAsync();

            // Generate time slots
            await GenerateSlotsAsync(Availability);

            TempData["SuccessMessage"] = "Availability added and slots generated.";
            return RedirectToPage("./Index");
        }

        private async Task GenerateSlotsAsync(InstructorAvailability availability)
        {
            var slots = new List<Reservation>();
            var currentDate = availability.StartDate.Date;

            while (currentDate <= availability.EndDate.Date)
            {
                var currentTime = availability.StartHour;

                while (currentTime.Add(TimeSpan.FromMinutes(availability.SlotDurationMinutes)) <= availability.EndHour)
                {
                    var startDateTime = currentDate.Add(currentTime);
                    var endDateTime = startDateTime.AddMinutes(availability.SlotDurationMinutes);

                    slots.Add(new Reservation
                    {
                        InstructorAvailabilityId = availability.Id,
                        StartTime = startDateTime,
                        EndTime = endDateTime,
                        StudentId = null,
                        IsActive = true
                    });

                    currentTime = currentTime.Add(TimeSpan.FromMinutes(availability.SlotDurationMinutes));
                }

                currentDate = currentDate.AddDays(1);
            }

            _context.Reservations.AddRange(slots);
            await _context.SaveChangesAsync();
        }

        private async Task LoadRoomSelectListAsync()
        {
            var rooms = await _context.Rooms
                .Where(r => r.IsActive)
                .OrderBy(r => r.RoomNumber)
                .ToListAsync();

            RoomSelectList = new SelectList(rooms, "Id", "Name");
        }
    }
}
