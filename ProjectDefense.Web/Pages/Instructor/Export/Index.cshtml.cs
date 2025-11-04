using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProjectDefense.Shared.Data;
using ProjectDefense.Shared.Entities;
using ProjectDefense.Web.Services.Export;
using System.ComponentModel.DataAnnotations;

namespace ProjectDefense.Web.Pages.Instructor.Export
{
    [Authorize(Roles = "Instructor")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ExportService _exportService;

        public IndexModel(ApplicationDbContext context, ExportService exportService)
        {
            _context = context;
            _exportService = exportService;
        }

        [BindProperty]
        [Required(ErrorMessage = "Please select a room.")]
        public int RoomId { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Please provide a start date.")]
        public DateTime StartDate { get; set; } = DateTime.UtcNow;

        [BindProperty]
        [Required(ErrorMessage = "Please provide an end date.")]
        public DateTime EndDate { get; set; } = DateTime.UtcNow.AddDays(7);

        public SelectList RoomsSelectList { get; set; }

        public async Task OnGetAsync()
        {
            await LoadRoomsSelectListAsync();
        }

        public async Task<IActionResult> OnPostExportTxtAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadRoomsSelectListAsync();
                return Page();
            }

            var (reservations, roomName) = await GetReservationsAsync();
            
            if (!reservations.Any())
            {
                ModelState.AddModelError("", "No reservations found for the selected criteria.");
                await LoadRoomsSelectListAsync();
                return Page();
            }

            var fileBytes = _exportService.ExportToTxt(reservations, roomName);
            return File(fileBytes, "text/plain", $"Reservations_{roomName}_{DateTime.Now:yyyyMMdd}.txt");
        }

        public async Task<IActionResult> OnPostExportExcelAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadRoomsSelectListAsync();
                return Page();
            }

            var (reservations, roomName) = await GetReservationsAsync();

            if (!reservations.Any())
            {
                ModelState.AddModelError("", "No reservations found for the selected criteria.");
                await LoadRoomsSelectListAsync();
                return Page();
            }

            var fileBytes = _exportService.ExportToExcel(reservations, roomName);
            return File(fileBytes, 
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"Reservations_{roomName}_{DateTime.Now:yyyyMMdd}.xlsx");
        }

        public async Task<IActionResult> OnPostExportPdfAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadRoomsSelectListAsync();
                return Page();
            }

            var (reservations, roomName) = await GetReservationsAsync();

            if (!reservations.Any())
            {
                ModelState.AddModelError("", "No reservations found for the selected criteria.");
                await LoadRoomsSelectListAsync();
                return Page();
            }

            var fileBytes = _exportService.ExportToPdf(reservations, roomName);
            return File(fileBytes, "application/pdf", $"Reservations_{roomName}_{DateTime.Now:yyyyMMdd}.pdf");
        }

        private async Task<(List<Reservation> reservations, string roomName)> GetReservationsAsync()
        {
            var room = await _context.Rooms.FindAsync(RoomId);
            var roomName = room?.Name ?? "Unknown";

            var utcStart = DateTime.SpecifyKind(StartDate.Date, DateTimeKind.Utc);
            var utcEnd = DateTime.SpecifyKind(EndDate.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);

            var reservations = await _context.Reservations
                .Include(r => r.InstructorAvailability)
                .ThenInclude(a => a.Room)
                .Include(r => r.Student)
                .Where(r => r.InstructorAvailability.RoomId == RoomId &&
                            r.StartTime >= utcStart &&
                            r.StartTime <= utcEnd)
                .OrderBy(r => r.StartTime)
                .ToListAsync();

            return (reservations, roomName);
        }

        private async Task LoadRoomsSelectListAsync()
        {
            var rooms = await _context.Rooms
                .Where(r => r.IsActive)
                .OrderBy(r => r.RoomNumber)
                .ToListAsync();

            RoomsSelectList = new SelectList(rooms, "Id", "Name");
        }
    }
}
