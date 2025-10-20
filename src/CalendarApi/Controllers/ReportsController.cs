using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CalendarApi.Data;
using CalendarApi.Models;

namespace CalendarApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly CalendarDbContext _context;

        public ReportsController(CalendarDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Relatório de eventos por período com agrupamentos
        /// </summary>
        [HttpGet("events-by-period")]
        public async Task<IActionResult> GetEventsByPeriod([FromQuery] string email, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            if (string.IsNullOrEmpty(email))
                return BadRequest(new { error = "email é obrigatório" });

            try
            {
                var events = await _context.Events
                    .Where(e => e.Email == email && e.Date >= startDate && e.Date <= endDate)
                    .ToListAsync();

                // Agrupamento por mês
                var eventsByMonth = events
                    .GroupBy(e => new { e.Date.Year, e.Date.Month })
                    .Select(g => new
                    {
                        Period = $"{g.Key.Year}-{g.Key.Month:D2}",
                        Count = g.Count(),
                        Events = g.Select(e => new
                        {
                            e.Id,
                            e.Title,
                            e.Date,
                            e.Type
                        }).ToList()
                    })
                    .OrderBy(x => x.Period)
                    .ToList();

                // Agrupamento por tipo
                var eventsByType = events
                    .Where(e => !string.IsNullOrEmpty(e.Type))
                    .GroupBy(e => e.Type)
                    .Select(g => new
                    {
                        Type = g.Key,
                        Count = g.Count(),
                        Percentage = Math.Round((double)g.Count() / events.Count * 100, 2)
                    })
                    .OrderByDescending(x => x.Count)
                    .ToList();

                // Eventos por dia da semana
                var eventsByDayOfWeek = events
                    .GroupBy(e => e.Date.DayOfWeek)
                    .Select(g => new
                    {
                        DayOfWeek = g.Key.ToString(),
                        Count = g.Count()
                    })
                    .OrderBy(x => x.DayOfWeek)
                    .ToList();

                return Ok(new
                {
                    Summary = new
                    {
                        TotalEvents = events.Count,
                        StartDate = startDate.ToString("yyyy-MM-dd"),
                        EndDate = endDate.ToString("yyyy-MM-dd")
                    },
                    EventsByMonth = eventsByMonth,
                    EventsByType = eventsByType,
                    EventsByDayOfWeek = eventsByDayOfWeek
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Erro interno do servidor", details = ex.Message });
            }
        }

        /// <summary>
        /// Análise de produtividade por cliente
        /// </summary>
        [HttpGet("client-productivity")]
        public async Task<IActionResult> GetClientProductivity([FromQuery] string email)
        {
            if (string.IsNullOrEmpty(email))
                return BadRequest(new { error = "email é obrigatório" });

            try
            {
                var events = await _context.Events
                    .Where(e => e.Email == email && !string.IsNullOrEmpty(e.Client))
                    .ToListAsync();

                var clientAnalysis = events
                    .GroupBy(e => e.Client)
                    .Select(g => new
                    {
                        Client = g.Key,
                        TotalEvents = g.Count(),
                        LastEvent = g.Max(e => e.Date),
                        FirstEvent = g.Min(e => e.Date),
                        EventTypes = g.Where(e => !string.IsNullOrEmpty(e.Type))
                                   .GroupBy(e => e.Type)
                                   .Select(et => new { Type = et.Key, Count = et.Count() })
                                   .ToList(),
                        AverageEventsPerMonth = Math.Round((double)g.Count() / 
                            Math.Max(1, (g.Max(e => e.Date) - g.Min(e => e.Date)).Days / 30.0), 2)
                    })
                    .OrderByDescending(x => x.TotalEvents)
                    .ToList();

                return Ok(new { data = clientAnalysis });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Erro interno do servidor", details = ex.Message });
            }
        }

        /// <summary>
        /// Análise de tendências temporais
        /// </summary>
        [HttpGet("temporal-trends")]
        public async Task<IActionResult> GetTemporalTrends([FromQuery] string email, [FromQuery] int months = 12)
        {
            if (string.IsNullOrEmpty(email))
                return BadRequest(new { error = "email é obrigatório" });

            try
            {
                var startDate = DateTime.Today.AddMonths(-months);
                var events = await _context.Events
                    .Where(e => e.Email == email && e.Date >= startDate)
                    .ToListAsync();

                // Tendência mensal
                var monthlyTrend = events
                    .GroupBy(e => new { e.Date.Year, e.Date.Month })
                    .Select(g => new
                    {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        Count = g.Count(),
                        Types = g.Where(e => !string.IsNullOrEmpty(e.Type))
                               .GroupBy(e => e.Type)
                               .Select(t => new { Type = t.Key, Count = t.Count() })
                               .ToList()
                    })
                    .OrderBy(x => x.Year)
                    .ThenBy(x => x.Month)
                    .ToList();

                // Horários mais comuns
                var timeAnalysis = events
                    .GroupBy(e => e.Time.Hours)
                    .Select(g => new
                    {
                        Hour = g.Key,
                        Count = g.Count(),
                        Percentage = Math.Round((double)g.Count() / events.Count * 100, 2)
                    })
                    .OrderByDescending(x => x.Count)
                    .ToList();

                // Análise de lembretes
                var reminderAnalysis = events
                    .Where(e => e.ReminderMinutes.HasValue)
                    .GroupBy(e => e.ReminderMinutes)
                    .Select(g => new
                    {
                        ReminderMinutes = g.Key,
                        Count = g.Count(),
                        Percentage = Math.Round((double)g.Count() / events.Count * 100, 2)
                    })
                    .OrderBy(x => x.ReminderMinutes)
                    .ToList();

                return Ok(new
                {
                    MonthlyTrend = monthlyTrend,
                    TimeAnalysis = timeAnalysis,
                    ReminderAnalysis = reminderAnalysis,
                    Summary = new
                    {
                        TotalEvents = events.Count,
                        AverageEventsPerMonth = Math.Round((double)events.Count / months, 2),
                        MostActiveMonth = monthlyTrend.OrderByDescending(x => x.Count).FirstOrDefault()?.Month,
                        MostActiveHour = timeAnalysis.FirstOrDefault()?.Hour
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Erro interno do servidor", details = ex.Message });
            }
        }

        /// <summary>
        /// Relatório de eventos com conflitos de horário
        /// </summary>
        [HttpGet("time-conflicts")]
        public async Task<IActionResult> GetTimeConflicts([FromQuery] string email, [FromQuery] DateTime? date = null)
        {
            if (string.IsNullOrEmpty(email))
                return BadRequest(new { error = "email é obrigatório" });

            try
            {
                var targetDate = date ?? DateTime.Today;
                var events = await _context.Events
                    .Where(e => e.Email == email && e.Date.Date == targetDate.Date)
                    .OrderBy(e => e.Time)
                    .ToListAsync();

                var conflicts = new List<object>();
                
                for (int i = 0; i < events.Count - 1; i++)
                {
                    var currentEvent = events[i];
                    var nextEvent = events[i + 1];
                    
                    var currentEndTime = currentEvent.Time.Add(TimeSpan.FromHours(1)); // Assumindo 1h de duração padrão
                    
                    if (currentEndTime > nextEvent.Time)
                    {
                        conflicts.Add(new
                        {
                            Event1 = new { currentEvent.Id, currentEvent.Title, currentEvent.Time },
                            Event2 = new { nextEvent.Id, nextEvent.Title, nextEvent.Time },
                            OverlapMinutes = (currentEndTime - nextEvent.Time).TotalMinutes
                        });
                    }
                }

                return Ok(new
                {
                    Date = targetDate.ToString("yyyy-MM-dd"),
                    TotalEvents = events.Count,
                    Conflicts = conflicts,
                    HasConflicts = conflicts.Any()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Erro interno do servidor", details = ex.Message });
            }
        }
    }
}
