using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProjectDefense.Shared.Data;
using ProjectDefense.Shared.Entities;

namespace ProjectDefense.Web.Pages.Student.Rezerwacje
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

        public List<Rezerwacja> HistoryReservations { get; set; } = new();

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            HistoryReservations = await _context.Rezerwacje
                .Include(r => r.DostepnoscProwadzacego).ThenInclude(d => d.Sala)
                .Where(r => r.StudentId == user.Id &&
                            r.IsActive &&
                            r.CzasZakonczenia <= DateTime.UtcNow)
                .OrderByDescending(r => r.CzasRozpoczecia)
                .Take(30)
                .ToListAsync();
        }
    }
}