using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProjectDefense.Shared.Data;
using ProjectDefense.Shared.Entities;
using ProjectDefense.Web.Services;
using System.ComponentModel.DataAnnotations;

namespace ProjectDefense.Web.Pages.Prowadzacy.Eksport
{
    [Authorize(Roles = "Prowadzący")]
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
        [Required(ErrorMessage = "Wybierz salę")]
        public int SalaId { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Podaj datę początkową")]
        public DateTime DataOd { get; set; } = DateTime.Today;

        [BindProperty]
        [Required(ErrorMessage = "Podaj datę końcową")]
        public DateTime DataDo { get; set; } = DateTime.Today.AddDays(7);

        public SelectList SaleSelectList { get; set; }

        public async Task OnGetAsync()
        {
            await LoadSaleSelectList();
        }

        public async Task<IActionResult> OnPostExportTxtAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadSaleSelectList();
                return Page();
            }

            var (rezerwacje, nazwaSali) = await GetRezerwacjeAsync();
            
            if (rezerwacje.Count == 0)
            {
                ModelState.AddModelError("", "Brak rezerwacji dla wybranych kryteriów.");
                await LoadSaleSelectList();
                return Page();
            }

            var fileBytes = _exportService.ExportToTxt(rezerwacje, nazwaSali);
            return File(fileBytes, "text/plain", $"Rezerwacje_{nazwaSali}_{DateTime.Now:yyyyMMdd}.txt");
        }

        public async Task<IActionResult> OnPostExportExcelAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadSaleSelectList();
                return Page();
            }

            var (rezerwacje, nazwaSali) = await GetRezerwacjeAsync();
            
            if (rezerwacje.Count == 0)
            {
                ModelState.AddModelError("", "Brak rezerwacji dla wybranych kryteriów.");
                await LoadSaleSelectList();
                return Page();
            }

            var fileBytes = _exportService.ExportToExcel(rezerwacje, nazwaSali);
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                       $"Rezerwacje_{nazwaSali}_{DateTime.Now:yyyyMMdd}.xlsx");
        }

        public async Task<IActionResult> OnPostExportPdfAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadSaleSelectList();
                return Page();
            }

            var (rezerwacje, nazwaSali) = await GetRezerwacjeAsync();
            
            if (rezerwacje.Count == 0)
            {
                ModelState.AddModelError("", "Brak rezerwacji dla wybranych kryteriów.");
                await LoadSaleSelectList();
                return Page();
            }

            var fileBytes = _exportService.ExportToPdf(rezerwacje, nazwaSali);
            return File(fileBytes, "application/pdf", $"Rezerwacje_{nazwaSali}_{DateTime.Now:yyyyMMdd}.pdf");
        }

        private async Task<(List<Rezerwacja> rezerwacje, string nazwaSali)> GetRezerwacjeAsync()
        {
            var sala = await _context.Sale.FindAsync(SalaId);
            var nazwaSali = sala?.Nazwa ?? "Unknown";

            var rezerwacje = await _context.Rezerwacje
                .Include(r => r.DostepnoscProwadzacego)
                .ThenInclude(d => d.Sala)
                .Include(r => r.Student)
                .Where(r => r.DostepnoscProwadzacego.SalaId == SalaId &&
                           r.CzasRozpoczecia.Date >= DataOd.Date &&
                           r.CzasRozpoczecia.Date <= DataDo.Date)
                .OrderBy(r => r.CzasRozpoczecia)
                .ToListAsync();

            return (rezerwacje, nazwaSali);
        }

        private async Task LoadSaleSelectList()
        {
            var sale = await _context.Sale
                .Where(s => s.IsActive)
                .OrderBy(s => s.NumerSali)
                .ToListAsync();
            
            SaleSelectList = new SelectList(sale, "Id", "Nazwa");
        }
    }
}
