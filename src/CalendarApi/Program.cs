using Microsoft.EntityFrameworkCore;
using CalendarApi.Data;
using CalendarApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { 
        Title = "Calendar API", 
        Version = "v1",
        Description = "API para gerenciamento de eventos de calendário com funcionalidades avançadas de pesquisa e estatísticas"
    });
});

// Register services
builder.Services.AddScoped<IEventService, EventService>();

// Configure SQLite (file-based). Change DataSource to ":memory:" for in-memory.
var connStr = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=calendar.db";
builder.Services.AddDbContext<CalendarDbContext>(options => options.UseSqlite(connStr));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
