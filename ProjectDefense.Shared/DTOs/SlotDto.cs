namespace ProjectDefense.Shared.DTOs;


public class SlotDto
{
    public int Id { get; set; }
    public int RoomId { get; set; }
    public string RoomName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsAvailable { get; set; }
    public string? StudentName { get; set; }
    public string InstructorName { get; set; } = string.Empty;
}