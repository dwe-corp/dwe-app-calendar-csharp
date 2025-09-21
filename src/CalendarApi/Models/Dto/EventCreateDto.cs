using System.ComponentModel.DataAnnotations;

namespace CalendarApi.Models.Dto
{
    public class EventCreateDto
    {
        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public TimeSpan Time { get; set; }

        public string? Client { get; set; }
        public string? Type { get; set; }
        public int? ReminderMinutes { get; set; }
        public string? Notes { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}
