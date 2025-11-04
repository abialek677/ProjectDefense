using ClosedXML.Excel;
using ProjectDefense.Shared.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ProjectDefense.Web.Services.Export
{
    public class ExportService
    {
        public byte[] ExportToTxt(List<Reservation> rezerwacje, string nazwaSali)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"Wykres rezerwacji - Sala: {nazwaSali}");
            sb.AppendLine($"Data wygenerowania: {DateTime.Now:dd.MM.yyyy HH:mm}");
            sb.AppendLine(new string('-', 80));
            sb.AppendLine();
            
            foreach (var rez in rezerwacje.OrderBy(r => r.StartTime))
            {
                var studentInfo = rez.Student != null 
                    ? $"{rez.Student.FirstName} {rez.Student.LastName}"
                    : "WOLNY";
                
                sb.AppendLine($"Data: {rez.StartTime:dd.MM.yyyy}");
                sb.AppendLine($"Godziny: {rez.StartTime:HH:mm} - {rez.EndTime:HH:mm}");
                sb.AppendLine($"Student: {studentInfo}");
                sb.AppendLine(new string('-', 40));
            }
            
            return System.Text.Encoding.UTF8.GetBytes(sb.ToString());
        }
        
        public byte[] ExportToExcel(List<Reservation> rezerwacje, string nazwaSali)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Rezerwacje");
            
            // Nagłówki
            worksheet.Cell(1, 1).Value = "Sala";
            worksheet.Cell(1, 2).Value = nazwaSali;
            worksheet.Cell(1, 1).Style.Font.Bold = true;
            
            worksheet.Cell(3, 1).Value = "Data";
            worksheet.Cell(3, 2).Value = "Początek";
            worksheet.Cell(3, 3).Value = "Koniec";
            worksheet.Cell(3, 4).Value = "Student";
            worksheet.Cell(3, 5).Value = "Email";
            
            // Stylowanie nagłówków
            var headerRange = worksheet.Range(3, 1, 3, 5);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thick;
            
            // Dane
            int currentRow = 4;
            foreach (var rez in rezerwacje.OrderBy(r => r.StartTime))
            {
                worksheet.Cell(currentRow, 1).Value = rez.StartTime.ToString("dd.MM.yyyy");
                worksheet.Cell(currentRow, 2).Value = rez.StartTime.ToString("HH:mm");
                worksheet.Cell(currentRow, 3).Value = rez.EndTime.ToString("HH:mm");
                worksheet.Cell(currentRow, 4).Value = rez.Student != null 
                    ? $"{rez.Student.FirstName} {rez.Student.LastName}" 
                    : "WOLNY";
                worksheet.Cell(currentRow, 5).Value = rez.Student?.Email ?? "";
                
                currentRow++;
            }
            
            // Autofit kolumn
            worksheet.Columns().AdjustToContents();
            
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
        
        public byte[] ExportToPdf(List<Reservation> rezerwacje, string nazwaSali)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11));
                    
                    page.Header()
                        .Column(column =>
                        {
                            column.Item().Text($"Wykres rezerwacji - Sala: {nazwaSali}")
                                .FontSize(20).Bold().FontColor(Colors.Blue.Darken2);
                            column.Item().Text($"Data: {DateTime.Now:dd.MM.yyyy HH:mm}")
                                .FontSize(10);
                        });
                    
                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(3);
                            });
                            
                            // Nagłówki
                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("Data");
                                header.Cell().Element(CellStyle).Text("Początek");
                                header.Cell().Element(CellStyle).Text("Koniec");
                                header.Cell().Element(CellStyle).Text("Student");
                                
                                static IContainer CellStyle(IContainer container)
                                {
                                    return container
                                        .Border(1)
                                        .BorderColor(Colors.Grey.Lighten1)
                                        .Background(Colors.Grey.Lighten3)
                                        .Padding(5)
                                        .AlignCenter()
                                        .AlignMiddle();
                                }
                            });
                            
                            // Dane
                            foreach (var rez in rezerwacje.OrderBy(r => r.StartTime))
                            {
                                table.Cell().Element(CellStyle).Text(rez.StartTime.ToString("dd.MM.yyyy"));
                                table.Cell().Element(CellStyle).Text(rez.StartTime.ToString("HH:mm"));
                                table.Cell().Element(CellStyle).Text(rez.EndTime.ToString("HH:mm"));
                                table.Cell().Element(CellStyle).Text(
                                    rez.Student != null 
                                        ? $"{rez.Student.FirstName} {rez.Student.LastName}"
                                        : "WOLNY");
                                
                                static IContainer CellStyle(IContainer container)
                                {
                                    return container
                                        .Border(1)
                                        .BorderColor(Colors.Grey.Lighten2)
                                        .Padding(5);
                                }
                            }
                        });
                    
                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Strona ");
                            x.CurrentPageNumber();
                            x.Span(" z ");
                            x.TotalPages();
                        });
                });
            });
            
            return document.GeneratePdf();
        }
    }
}
