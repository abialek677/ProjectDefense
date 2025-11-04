using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProjectDefense.Shared.Data;
using ProjectDefense.Shared.Entities;

namespace ProjectDefense.Web.Pages.Student.Reservations
{
    [Authorize(Roles = "Student")]
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
        public int RoomId { get; set; }

        [BindProperty]
        public DateTime Date { get; set; }

        [BindProperty]
        public int SlotId { get; set; }

        public SelectList RoomSelectList { get; set; }
        public SelectList SlotSelectList { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            RoomSelectList = new SelectList(
                await _context.Rooms
                    .Where(r => r.IsActive)
                    .OrderBy(r => r.Name)
                    .ToListAsync(),
                "Id", "Name"
            );

            // Initially, SlotSelectList can be empty.
            SlotSelectList = new SelectList(new List<SelectListItem>(), "Value", "Text");
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            RoomSelectList = new SelectList(
                await _context.Rooms
                    .Where(r => r.IsActive)
                    .OrderBy(r => r.Name)
                    .ToListAsync(),
                "Id", "Name"
            );

            if (!ModelState.IsValid)
            {
                SlotSelectList = new SelectList(new List<SelectListItem>(), "Value", "Text");
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);

            var isBlocked = await _context.StudentBlocks
                .AnyAsync(b => b.StudentId == user.Id && b.IsActive);

            if (isBlocked)
            {
                ModelState.AddModelError(string.Empty, 
                    "Your account has been blocked by an instructor. Please contact the administration.");
                return Page();
            }

            var slot = await _context.Reservations
                .Include(r => r.InstructorAvailability)
                .FirstOrDefaultAsync(r =>
                    r.Id == SlotId &&
                    r.InstructorAvailability.RoomId == RoomId &&
                    r.StartTime.Date == Date.Date &&
                    r.IsActive &&
                    r.StudentId == null);

            if (slot == null)
            {
                TempData["ErrorMessage"] = "The selected slot is no longer available. Please choose another.";
                SlotSelectList = new SelectList(new List<SelectListItem>(), "Value", "Text");
                return Page();
            }

            // Check if the student already has an active reservation
            var alreadyReserved = await _context.Reservations
                .AnyAsync(r => r.StudentId == user.Id && r.IsActive);

            if (alreadyReserved)
            {
                TempData["ErrorMessage"] = "You already have an active reservation. Please cancel it first.";
                SlotSelectList = new SelectList(new List<SelectListItem>(), "Value", "Text");
                return Page();
            }

            slot.StudentId = user.Id;
            slot.ReservationDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Reservation has been successfully created!";
            return RedirectToPage("Index");
        }
    }
}
