using System.ComponentModel.DataAnnotations;

namespace CalendarApi.Models
{
    public class EventItem
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        // Date only (date component)
        public DateTime Date { get; set; }

        // Time of day
        public TimeSpan Time { get; set; }

        public string? Client { get; set; }
        public string? Type { get; set; }

        // Minutes before event
        public int? ReminderMinutes { get; set; }
        public string? Notes { get; set; }

        [Required]
        public string Email { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
