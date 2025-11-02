namespace ProjectDefense.Shared.DTOs;

public class SlotDto
{
    public int Id { get; set; }
    public int SalaId { get; set; }
    public string NazwaSali { get; set; } = string.Empty;
    public DateTime CzasRozpoczecia { get; set; }
    public DateTime CzasZakonczenia { get; set; }
    public bool IsAvailable { get; set; }
    public string? StudentName { get; set; }
    public string ProwadzacyName { get; set; } = string.Empty;
}