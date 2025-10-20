namespace CalendarApi.Models.Dto
{
    public class EventResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public TimeSpan Time { get; set; }
        public string? Client { get; set; }
        public string? Type { get; set; }
        public int? ReminderMinutes { get; set; }
        public string? Notes { get; set; }
        public string Email { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
