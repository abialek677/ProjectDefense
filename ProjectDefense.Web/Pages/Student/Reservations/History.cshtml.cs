using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProjectDefense.Shared.Data;
using ProjectDefense.Shared.Entities;

namespace ProjectDefense.Web.Pages.Student.Reservations
{
    [Authorize(Roles = "Student")]
    public class HistoryModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HistoryModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public List<Reservation> HistoryReservations { get; set; } = new();

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            HistoryReservations = await _context.Reservations
                .Include(r => r.InstructorAvailability)
                .ThenInclude(a => a.Room)
                .Where(r =>
                    r.StudentId == user.Id &&
                    r.IsActive &&
                    r.EndTime <= DateTime.UtcNow)
                .OrderByDescending(r => r.StartTime)
                .Take(30)
                .ToListAsync();
        }
    }
}