using System.ComponentModel.DataAnnotations;

namespace CalendarApi.Models.Dto
{
    public class EventUpdateDto
    {
        [Required(ErrorMessage = "Título é obrigatório")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Título deve ter entre 3 e 200 caracteres")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Data é obrigatória")]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Hora é obrigatória")]
        public TimeSpan Time { get; set; }

        [StringLength(100, ErrorMessage = "Cliente deve ter no máximo 100 caracteres")]
        public string? Client { get; set; }

        [StringLength(50, ErrorMessage = "Tipo deve ter no máximo 50 caracteres")]
        public string? Type { get; set; }

        [Range(0, 1440, ErrorMessage = "Lembrete deve estar entre 0 e 1440 minutos")]
        public int? ReminderMinutes { get; set; }

        [StringLength(500, ErrorMessage = "Notas devem ter no máximo 500 caracteres")]
        public string? Notes { get; set; }

        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email deve ter formato válido")]
        public string Email { get; set; } = string.Empty;
    }
}
