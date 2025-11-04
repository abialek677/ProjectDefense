using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProjectDefense.Shared.Data;
using ProjectDefense.Shared.Entities;
using System.ComponentModel.DataAnnotations;

namespace ProjectDefense.Web.Pages.Instructor.Blocks
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
        [Required(ErrorMessage = "Please select a student.")]
        public string StudentId { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Please provide a reason for the ban.")]
        [StringLength(500, ErrorMessage = "The reason cannot exceed 500 characters.")]
        public string Reason { get; set; }

        public SelectList StudentsSelectList { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            await LoadStudentsSelectListAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadStudentsSelectListAsync();
                return Page();
            }

            // Check if student is already banned
            var alreadyBanned = await _context.StudentBlocks
                .AnyAsync(b => b.StudentId == StudentId && b.IsActive);

            if (alreadyBanned)
            {
                ModelState.AddModelError("", "This student is already banned.");
                await LoadStudentsSelectListAsync();
                return Page();
            }

            var instructor = await _userManager.GetUserAsync(User);

            var block = new StudentBlock
            {
                StudentId = StudentId,
                BlockReason = Reason,
                BlockingInstructorId = instructor.Id,
                BlockDate = DateTime.UtcNow,
                IsActive = true
            };

            _context.StudentBlocks.Add(block);

            // Cancel all future reservations of the student
            var reservations = await _context.Reservations
                .Where(r => r.StudentId == StudentId && r.IsActive && r.StartTime > DateTime.UtcNow)
                .ToListAsync();

            foreach (var reservation in reservations)
            {
                reservation.StudentId = null;
                reservation.ReservationDate = null;
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Student has been banned and their reservations were canceled.";
            return RedirectToPage("./Index");
        }

        private async Task LoadStudentsSelectListAsync()
        {
            var students = await _userManager.GetUsersInRoleAsync("Student");
            StudentsSelectList = new SelectList(
                students.Select(s => new { Id = s.Id, Name = $"{s.FirstName} {s.LastName} ({s.Email})" }),
                "Id", "Name");
        }
    }
}
