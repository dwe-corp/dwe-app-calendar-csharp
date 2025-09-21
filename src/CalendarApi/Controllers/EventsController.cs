using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CalendarApi.Data;
using CalendarApi.Models;
using CalendarApi.Models.Dto;
using System.Text.Json;

namespace CalendarApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly CalendarDbContext _db;
        public EventsController(CalendarDbContext db) => _db = db;

        [HttpGet]
        public async Task<IActionResult> GetByEmail([FromQuery] string email)
        {
            if (string.IsNullOrEmpty(email)) return BadRequest(new { error = "email é obrigatório" });

            var list = await _db.Events
                .Where(e => e.Email == email)
                .OrderBy(e => e.Date).ThenBy(e => e.Time)
                .ToListAsync();

            var formatted = new
            {
                data = list.Select(e => new
                {
                    Titulo = e.Title,
                    Data = e.Date.ToString("yyyy-MM-dd"),
                    Hora = e.Time.ToString(),
                    Cliente = e.Client,
                    Tipo = e.Type,
                    Lembrete = e.ReminderMinutes?.ToString() ?? string.Empty,
                    Notas = e.Notes,
                    email = e.Email
                }).ToArray()
            };

            return Ok(formatted);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _db.Events.FindAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] EventCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var item = new EventItem
            {
                Title = dto.Title,
                Date = dto.Date.Date,
                Time = dto.Time,
                Client = dto.Client,
                Type = dto.Type,
                ReminderMinutes = dto.ReminderMinutes,
                Notes = dto.Notes,
                Email = dto.Email,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.Events.Add(item);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] EventCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var item = await _db.Events.FindAsync(id);
            if (item == null) return NotFound();

            item.Title = dto.Title;
            item.Date = dto.Date.Date;
            item.Time = dto.Time;
            item.Client = dto.Client;
            item.Type = dto.Type;
            item.ReminderMinutes = dto.ReminderMinutes;
            item.Notes = dto.Notes;
            item.Email = dto.Email;
            item.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _db.Events.FindAsync(id);
            if (item == null) return NotFound();
            _db.Events.Remove(item);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("export")]
        public async Task<IActionResult> Export([FromQuery] string email)
        {
            if (string.IsNullOrEmpty(email)) return BadRequest(new { error = "email é obrigatório" });
            var list = await _db.Events.Where(e => e.Email == email).OrderBy(e=>e.Date).ThenBy(e=>e.Time).ToListAsync();
            var formatted = new { data = list.Select(e => new {
                Titulo = e.Title,
                Data = e.Date.ToString("yyyy-MM-dd"),
                Hora = e.Time.ToString(),
                Cliente = e.Client,
                Tipo = e.Type,
                Lembrete = e.ReminderMinutes?.ToString() ?? string.Empty,
                Notas = e.Notes,
                email = e.Email
            }) };

            var json = System.Text.Json.JsonSerializer.Serialize(formatted, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);
            return File(bytes, "application/json", $"events_{email}.json");
        }

        [HttpPost("import")]
        public async Task<IActionResult> Import([FromBody] JsonElement payload)
        {
            // Expecting { "data": [ { ... } ] }
            if (!payload.TryGetProperty("data", out var data)) return BadRequest(new { error = "payload inválido, espere { data: [ ... ] }" });

            var created = 0;
            foreach (var el in data.EnumerateArray())
            {
                try
                {
                    var title = el.GetProperty("Titulo").GetString() ?? string.Empty;
                    var dateStr = el.GetProperty("Data").GetString() ?? string.Empty;
                    var timeStr = el.GetProperty("Hora").GetString() ?? string.Empty;
                    var email = el.GetProperty("email").GetString() ?? string.Empty;

                    if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(dateStr) || string.IsNullOrEmpty(timeStr) || string.IsNullOrEmpty(email)) continue;

                    var date = DateTime.Parse(dateStr);
                    var time = TimeSpan.Parse(timeStr);

                    var item = new EventItem
                    {
                        Title = title,
                        Date = date.Date,
                        Time = time,
                        Client = el.TryGetProperty("Cliente", out var c) ? c.GetString() : null,
                        Type = el.TryGetProperty("Tipo", out var t) ? t.GetString() : null,
                        Notes = el.TryGetProperty("Notas", out var n) ? n.GetString() : null,
                        Email = email,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    if (el.TryGetProperty("Lembrete", out var lemb) && lemb.ValueKind == JsonValueKind.String)
                    {
                        if (int.TryParse(lemb.GetString(), out var mins)) item.ReminderMinutes = mins;
                    }

                    _db.Events.Add(item);
                    created++;
                }
                catch { /* ignore malformed entries */ }
            }

            await _db.SaveChangesAsync();
            return Ok(new { created });
        }
    }
}
