using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CalendarApi.Data;
using CalendarApi.Models;
using CalendarApi.Models.Dto;
using CalendarApi.Services;
using System.Text.Json;

namespace CalendarApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly IEventService _eventService;
        private readonly CalendarDbContext _db;

        public EventsController(IEventService eventService, CalendarDbContext db)
        {
            _eventService = eventService;
            _db = db;
        }

        /// <summary>
        /// Lista eventos por email
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetByEmail([FromQuery] string email)
        {
            if (string.IsNullOrEmpty(email)) 
                return BadRequest(new { error = "email é obrigatório" });

            try
            {
                var events = await _eventService.GetByEmailAsync(email);
                return Ok(new { data = events });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Erro interno do servidor", details = ex.Message });
            }
        }

        /// <summary>
        /// Pesquisa avançada de eventos com filtros e paginação
        /// </summary>
        [HttpPost("search")]
        public async Task<IActionResult> SearchEvents([FromBody] EventSearchDto searchDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _eventService.SearchEventsAsync(searchDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Erro interno do servidor", details = ex.Message });
            }
        }

        /// <summary>
        /// Obtém eventos próximos (próximos 7 dias por padrão)
        /// </summary>
        [HttpGet("upcoming")]
        public async Task<IActionResult> GetUpcomingEvents([FromQuery] string email, [FromQuery] int days = 7)
        {
            if (string.IsNullOrEmpty(email))
                return BadRequest(new { error = "email é obrigatório" });

            try
            {
                var events = await _eventService.GetUpcomingEventsAsync(email, days);
                return Ok(new { data = events });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Erro interno do servidor", details = ex.Message });
            }
        }

        /// <summary>
        /// Obtém eventos por tipo
        /// </summary>
        [HttpGet("by-type")]
        public async Task<IActionResult> GetEventsByType([FromQuery] string email, [FromQuery] string type)
        {
            if (string.IsNullOrEmpty(email))
                return BadRequest(new { error = "email é obrigatório" });
            if (string.IsNullOrEmpty(type))
                return BadRequest(new { error = "tipo é obrigatório" });

            try
            {
                var events = await _eventService.GetEventsByTypeAsync(email, type);
                return Ok(new { data = events });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Erro interno do servidor", details = ex.Message });
            }
        }

        /// <summary>
        /// Obtém eventos por cliente
        /// </summary>
        [HttpGet("by-client")]
        public async Task<IActionResult> GetEventsByClient([FromQuery] string email, [FromQuery] string client)
        {
            if (string.IsNullOrEmpty(email))
                return BadRequest(new { error = "email é obrigatório" });
            if (string.IsNullOrEmpty(client))
                return BadRequest(new { error = "cliente é obrigatório" });

            try
            {
                var events = await _eventService.GetEventsByClientAsync(email, client);
                return Ok(new { data = events });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Erro interno do servidor", details = ex.Message });
            }
        }

        /// <summary>
        /// Obtém estatísticas dos eventos
        /// </summary>
        [HttpGet("statistics")]
        public async Task<IActionResult> GetEventStatistics([FromQuery] string email)
        {
            if (string.IsNullOrEmpty(email))
                return BadRequest(new { error = "email é obrigatório" });

            try
            {
                var statistics = await _eventService.GetEventStatisticsAsync(email);
                return Ok(new { data = statistics });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Erro interno do servidor", details = ex.Message });
            }
        }

        /// <summary>
        /// Obtém evento por ID
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var eventItem = await _eventService.GetByIdAsync(id);
                if (eventItem == null) return NotFound(new { error = "Evento não encontrado" });
                return Ok(eventItem);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Erro interno do servidor", details = ex.Message });
            }
        }

        /// <summary>
        /// Cria novo evento
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] EventCreateDto dto)
        {
            if (!ModelState.IsValid) 
                return BadRequest(ModelState);

            try
            {
                var eventItem = await _eventService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = eventItem.Id }, eventItem);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Erro interno do servidor", details = ex.Message });
            }
        }

        /// <summary>
        /// Atualiza evento existente
        /// </summary>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] EventUpdateDto dto)
        {
            if (!ModelState.IsValid) 
                return BadRequest(ModelState);

            try
            {
                var eventItem = await _eventService.UpdateAsync(id, dto);
                if (eventItem == null) 
                    return NotFound(new { error = "Evento não encontrado" });
                
                return Ok(eventItem);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Erro interno do servidor", details = ex.Message });
            }
        }

        /// <summary>
        /// Remove evento
        /// </summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var deleted = await _eventService.DeleteAsync(id);
                if (!deleted) 
                    return NotFound(new { error = "Evento não encontrado" });
                
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Erro interno do servidor", details = ex.Message });
            }
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
