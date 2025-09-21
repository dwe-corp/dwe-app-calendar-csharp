using Microsoft.EntityFrameworkCore;
using CalendarApi.Models;

namespace CalendarApi.Data
{
    public class CalendarDbContext : DbContext
    {
        public CalendarDbContext(DbContextOptions<CalendarDbContext> options) : base(options) { }

        public DbSet<EventItem> Events => Set<EventItem>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EventItem>(b =>
            {
                b.HasKey(e => e.Id);
                b.Property(e => e.Title).IsRequired();
                b.Property(e => e.Date).IsRequired();
                b.Property(e => e.Time).HasConversion(
                    v => v.ToString(),
                    v => TimeSpan.Parse(v))
                    .HasColumnType("TEXT");
                b.HasIndex(e => e.Email);
            });
        }
    }
}
