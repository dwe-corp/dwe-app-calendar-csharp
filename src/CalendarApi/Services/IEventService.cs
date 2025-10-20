using CalendarApi.Models;
using CalendarApi.Models.Dto;

namespace CalendarApi.Services
{
    public interface IEventService
    {
        Task<EventResponseDto?> GetByIdAsync(int id);
        Task<IEnumerable<EventResponseDto>> GetByEmailAsync(string email);
        Task<PagedResult<EventResponseDto>> SearchEventsAsync(EventSearchDto searchDto);
        Task<EventResponseDto> CreateAsync(EventCreateDto createDto);
        Task<EventResponseDto?> UpdateAsync(int id, EventUpdateDto updateDto);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<EventResponseDto>> GetUpcomingEventsAsync(string email, int days = 7);
        Task<IEnumerable<EventResponseDto>> GetEventsByTypeAsync(string email, string type);
        Task<IEnumerable<EventResponseDto>> GetEventsByClientAsync(string email, string client);
        Task<Dictionary<string, int>> GetEventStatisticsAsync(string email);
    }

    public class PagedResult<T>
    {
        public IEnumerable<T> Data { get; set; } = Enumerable.Empty<T>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }
}
