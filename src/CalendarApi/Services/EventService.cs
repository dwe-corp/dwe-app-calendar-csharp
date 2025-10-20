using Microsoft.EntityFrameworkCore;
using CalendarApi.Data;
using CalendarApi.Models;
using CalendarApi.Models.Dto;

namespace CalendarApi.Services
{
    public class EventService : IEventService
    {
        private readonly CalendarDbContext _context;

        public EventService(CalendarDbContext context)
        {
            _context = context;
        }

        public async Task<EventResponseDto?> GetByIdAsync(int id)
        {
            var eventItem = await _context.Events.FindAsync(id);
            return eventItem != null ? MapToResponseDto(eventItem) : null;
        }

        public async Task<IEnumerable<EventResponseDto>> GetByEmailAsync(string email)
        {
            var events = await _context.Events
                .Where(e => e.Email == email)
                .OrderBy(e => e.Date)
                .ThenBy(e => e.Time)
                .ToListAsync();

            return events.Select(MapToResponseDto);
        }

        public async Task<PagedResult<EventResponseDto>> SearchEventsAsync(EventSearchDto searchDto)
        {
            var query = _context.Events.Where(e => e.Email == searchDto.Email);

            // Aplicar filtros
            if (searchDto.StartDate.HasValue)
                query = query.Where(e => e.Date >= searchDto.StartDate.Value);

            if (searchDto.EndDate.HasValue)
                query = query.Where(e => e.Date <= searchDto.EndDate.Value);

            if (!string.IsNullOrEmpty(searchDto.Type))
                query = query.Where(e => e.Type != null && e.Type.Contains(searchDto.Type));

            if (!string.IsNullOrEmpty(searchDto.Client))
                query = query.Where(e => e.Client != null && e.Client.Contains(searchDto.Client));

            if (!string.IsNullOrEmpty(searchDto.SearchTerm))
            {
                var searchTerm = searchDto.SearchTerm.ToLower();
                query = query.Where(e => 
                    e.Title.ToLower().Contains(searchTerm) ||
                    (e.Notes != null && e.Notes.ToLower().Contains(searchTerm)) ||
                    (e.Client != null && e.Client.ToLower().Contains(searchTerm)));
            }

            // Aplicar ordenação
            query = searchDto.SortBy.ToLower() switch
            {
                "title" => searchDto.SortDirection.ToLower() == "desc" 
                    ? query.OrderByDescending(e => e.Title) 
                    : query.OrderBy(e => e.Title),
                "type" => searchDto.SortDirection.ToLower() == "desc" 
                    ? query.OrderByDescending(e => e.Type) 
                    : query.OrderBy(e => e.Type),
                "client" => searchDto.SortDirection.ToLower() == "desc" 
                    ? query.OrderByDescending(e => e.Client) 
                    : query.OrderBy(e => e.Client),
                _ => searchDto.SortDirection.ToLower() == "desc" 
                    ? query.OrderByDescending(e => e.Date).ThenByDescending(e => e.Time)
                    : query.OrderBy(e => e.Date).ThenBy(e => e.Time)
            };

            var totalCount = await query.CountAsync();

            // Aplicar paginação
            var events = await query
                .Skip((searchDto.Page - 1) * searchDto.PageSize)
                .Take(searchDto.PageSize)
                .ToListAsync();

            return new PagedResult<EventResponseDto>
            {
                Data = events.Select(MapToResponseDto),
                TotalCount = totalCount,
                Page = searchDto.Page,
                PageSize = searchDto.PageSize
            };
        }

        public async Task<EventResponseDto> CreateAsync(EventCreateDto createDto)
        {
            var eventItem = new EventItem
            {
                Title = createDto.Title,
                Date = createDto.Date.Date,
                Time = createDto.Time,
                Client = createDto.Client,
                Type = createDto.Type,
                ReminderMinutes = createDto.ReminderMinutes,
                Notes = createDto.Notes,
                Email = createDto.Email,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Events.Add(eventItem);
            await _context.SaveChangesAsync();

            return MapToResponseDto(eventItem);
        }

        public async Task<EventResponseDto?> UpdateAsync(int id, EventUpdateDto updateDto)
        {
            var eventItem = await _context.Events.FindAsync(id);
            if (eventItem == null) return null;

            eventItem.Title = updateDto.Title;
            eventItem.Date = updateDto.Date.Date;
            eventItem.Time = updateDto.Time;
            eventItem.Client = updateDto.Client;
            eventItem.Type = updateDto.Type;
            eventItem.ReminderMinutes = updateDto.ReminderMinutes;
            eventItem.Notes = updateDto.Notes;
            eventItem.Email = updateDto.Email;
            eventItem.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return MapToResponseDto(eventItem);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var eventItem = await _context.Events.FindAsync(id);
            if (eventItem == null) return false;

            _context.Events.Remove(eventItem);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<EventResponseDto>> GetUpcomingEventsAsync(string email, int days = 7)
        {
            var startDate = DateTime.Today;
            var endDate = startDate.AddDays(days);

            var events = await _context.Events
                .Where(e => e.Email == email && e.Date >= startDate && e.Date <= endDate)
                .OrderBy(e => e.Date)
                .ThenBy(e => e.Time)
                .ToListAsync();

            return events.Select(MapToResponseDto);
        }

        public async Task<IEnumerable<EventResponseDto>> GetEventsByTypeAsync(string email, string type)
        {
            var events = await _context.Events
                .Where(e => e.Email == email && e.Type != null && e.Type.Contains(type))
                .OrderBy(e => e.Date)
                .ThenBy(e => e.Time)
                .ToListAsync();

            return events.Select(MapToResponseDto);
        }

        public async Task<IEnumerable<EventResponseDto>> GetEventsByClientAsync(string email, string client)
        {
            var events = await _context.Events
                .Where(e => e.Email == email && e.Client != null && e.Client.Contains(client))
                .OrderBy(e => e.Date)
                .ThenBy(e => e.Time)
                .ToListAsync();

            return events.Select(MapToResponseDto);
        }

        public async Task<Dictionary<string, int>> GetEventStatisticsAsync(string email)
        {
            var events = await _context.Events
                .Where(e => e.Email == email)
                .ToListAsync();

            var statistics = new Dictionary<string, int>
            {
                ["Total"] = events.Count,
                ["ThisMonth"] = events.Count(e => e.Date.Month == DateTime.Now.Month && e.Date.Year == DateTime.Now.Year),
                ["ThisWeek"] = events.Count(e => e.Date >= DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek) && e.Date <= DateTime.Today.AddDays(7 - (int)DateTime.Today.DayOfWeek)),
                ["Upcoming"] = events.Count(e => e.Date >= DateTime.Today)
            };

            // Estatísticas por tipo
            var typeStats = events
                .Where(e => !string.IsNullOrEmpty(e.Type))
                .GroupBy(e => e.Type)
                .ToDictionary(g => g.Key!, g => g.Count());

            foreach (var stat in typeStats)
            {
                statistics[$"Type_{stat.Key}"] = stat.Value;
            }

            return statistics;
        }

        private static EventResponseDto MapToResponseDto(EventItem eventItem)
        {
            return new EventResponseDto
            {
                Id = eventItem.Id,
                Title = eventItem.Title,
                Date = eventItem.Date,
                Time = eventItem.Time,
                Client = eventItem.Client,
                Type = eventItem.Type,
                ReminderMinutes = eventItem.ReminderMinutes,
                Notes = eventItem.Notes,
                Email = eventItem.Email,
                CreatedAt = eventItem.CreatedAt,
                UpdatedAt = eventItem.UpdatedAt
            };
        }
    }
}
