# 🗓️ CalendarApi - Sprint 4
**API de calendário** em ASP.NET Core 8.0 + EF Core (SQLite) com funcionalidades avançadas 🖥️

Este projeto evoluiu da Sprint 3 para incluir **CRUD completo**, **pesquisas avançadas com LINQ**, **relatórios analíticos** e **arquitetura bem estruturada**.

---
## 👥 DWE - Team
- Deivison Pertel – RM 550803
- Eduardo Akira Murata – RM 98713
- Wesley Souza de Oliveira – RM 97874

## 🎯 Funcionalidades da Sprint 4

### ✅ CRUD Completo 
- Validações robustas com Data Annotations
- Tratamento de erros padronizado
- DTOs específicos para cada operação
- Service Layer para separação de responsabilidades

### ✅ Pesquisas Avançadas com LINQ 
- Filtros dinâmicos e paginação
- Ordenação flexível
- Agrupamentos e agregações
- Análises temporais complexas

### ✅ Documentação Completa 
- Documentação detalhada da API
- Exemplos de uso
- Guia de arquitetura

### ✅ Arquitetura Estruturada
- Padrões de design implementados
- Injeção de dependência
- Separação de responsabilidades

## 🚀 Como Executar

### Pré-requisitos
- .NET 8.0 SDK
- Visual Studio 2022 ou VS Code

### Comandos
```bash
# 1. Restaurar pacotes
dotnet restore

# 2. Aplicar migrações
dotnet ef database update -p src/CalendarApi -s src/CalendarApi

# 3. Executar aplicação
dotnet run --project src/CalendarApi
```

### Acesso
- **API Base**: https://localhost:5001/api
- **Swagger UI**: https://localhost:5001/swagger

## 📌 Endpoints Principais

### CRUD Básico
- `GET /api/events?email={email}` → Listar eventos por e-mail
- `GET /api/events/{id}` → Obter evento por ID
- `POST /api/events` → Criar evento
- `PUT /api/events/{id}` → Atualizar evento
- `DELETE /api/events/{id}` → Deletar evento

### Pesquisas Avançadas
- `POST /api/events/search` → Pesquisa com filtros e paginação
- `GET /api/events/upcoming?email={email}&days={days}` → Eventos próximos
- `GET /api/events/by-type?email={email}&type={type}` → Eventos por tipo
- `GET /api/events/by-client?email={email}&client={client}` → Eventos por cliente
- `GET /api/events/statistics?email={email}` → Estatísticas dos eventos

### Relatórios Analíticos
- `GET /api/reports/events-by-period` → Relatório por período
- `GET /api/reports/client-productivity` → Análise de produtividade
- `GET /api/reports/temporal-trends` → Tendências temporais
- `GET /api/reports/time-conflicts` → Conflitos de horário

### Import/Export
- `GET /api/events/export?email={email}` → Exportar eventos em JSON
- `POST /api/events/import` → Importar eventos em JSON

## 🏗️ Arquitetura

### Padrões Utilizados
- **Repository Pattern** (via Service Layer)
- **DTO Pattern** (Data Transfer Objects)
- **Dependency Injection**
- **Entity Framework Core** com Code First

### Estrutura do Projeto
```
src/CalendarApi/
├── Controllers/          # Controladores da API
│   ├── EventsController.cs
│   └── ReportsController.cs
├── Data/                # Contexto do banco de dados
│   └── CalendarDbContext.cs
├── Models/              # Entidades e DTOs
│   ├── EventItem.cs
│   └── Dto/
│       ├── EventCreateDto.cs
│       ├── EventUpdateDto.cs
│       ├── EventResponseDto.cs
│       └── EventSearchDto.cs
├── Services/            # Lógica de negócio
│   ├── IEventService.cs
│   └── EventService.cs
└── Migrations/          # Migrações do banco
```

## 📊 Modelos de Dados

### EventItem (Entidade Principal)
```csharp
public class EventItem
{
    public int Id { get; set; }
    [Required] public string Title { get; set; }
    [Required] public DateTime Date { get; set; }
    [Required] public TimeSpan Time { get; set; }
    public string? Client { get; set; }
    public string? Type { get; set; }
    public int? ReminderMinutes { get; set; }
    public string? Notes { get; set; }
    [Required] public string Email { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

### DTOs de Entrada
- **EventCreateDto**: Para criação de eventos
- **EventUpdateDto**: Para atualização de eventos
- **EventSearchDto**: Para pesquisas avançadas

### DTOs de Saída
- **EventResponseDto**: Resposta padronizada de eventos
- **PagedResult<T>**: Para resultados paginados

## 📈 Exemplos de Uso

### 1. Criar Evento
```http
POST /api/events
Content-Type: application/json

{
  "title": "Reunião com Cliente",
  "date": "2024-01-15",
  "time": "14:30:00",
  "client": "Empresa ABC",
  "type": "Reunião",
  "reminderMinutes": 15,
  "notes": "Discussão sobre projeto",
  "email": "usuario@exemplo.com"
}
```

### 2. Pesquisa Avançada
```http
POST /api/events/search
Content-Type: application/json

{
  "email": "usuario@exemplo.com",
  "startDate": "2024-01-01",
  "endDate": "2024-01-31",
  "type": "Reunião",
  "searchTerm": "cliente",
  "page": 1,
  "pageSize": 10,
  "sortBy": "Date",
  "sortDirection": "asc"
}
```

### 3. Relatório de Produtividade
```http
GET /api/reports/client-productivity?email=usuario@exemplo.com
```

## 🛡️ Tratamento de Erros

### Códigos de Status HTTP
- `200 OK` - Sucesso
- `201 Created` - Recurso criado
- `204 No Content` - Sucesso sem conteúdo
- `400 Bad Request` - Dados inválidos
- `404 Not Found` - Recurso não encontrado
- `500 Internal Server Error` - Erro interno

### Formato de Resposta de Erro
```json
{
  "error": "Descrição do erro",
  "details": "Detalhes técnicos (apenas em desenvolvimento)"
}
```

## 🔍 Funcionalidades LINQ Demonstradas

### 1. Filtros Complexos
```csharp
var query = _context.Events.Where(e => e.Email == searchDto.Email);

if (searchDto.StartDate.HasValue)
    query = query.Where(e => e.Date >= searchDto.StartDate.Value);

if (!string.IsNullOrEmpty(searchDto.SearchTerm))
{
    var searchTerm = searchDto.SearchTerm.ToLower();
    query = query.Where(e => 
        e.Title.ToLower().Contains(searchTerm) ||
        (e.Notes != null && e.Notes.ToLower().Contains(searchTerm)));
}
```

### 2. Agrupamentos e Agregações
```csharp
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
```

### 3. Análises Temporais
```csharp
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
```

## 📊 Tecnologias

- **ASP.NET Core 8.0**
- **Entity Framework Core 7.0**
- **SQLite** (banco de dados)
- **Swagger/OpenAPI** (documentação)
- **LINQ** (consultas avançadas)

## 🚀 Próximos Passos

Para evoluir ainda mais o projeto, considere:
1. **Autenticação e Autorização** (JWT)
2. **Logging estruturado** (Serilog)
3. **Testes unitários** (xUnit)
4. **Cache** (Redis/Memory)
5. **Rate Limiting**
6. **Health Checks**
7. **Docker** para containerização

## 📚 Documentação Adicional

Para diagramas de arquitetura detalhados, consulte:
- [ARCHITECTURE_DIAGRAMS.md](./ARCHITECTURE_DIAGRAMS.md) - Diagramas Mermaid da arquitetura
- [Swagger UI](https://localhost:5001/swagger) - Interface interativa da API

## ⚙️ Observações

- Campos de data/hora mapeados para **DateTime/TimeSpan**
- Banco padrão: **SQLite** (`calendar.db`)
- Validações robustas em todos os endpoints
- Tratamento de erros padronizado
- Arquitetura escalável e manutenível

---

**Desenvolvido por:** DWE Team  
**Sprint:** 4  
**Tecnologias:** ASP.NET Core 8.0, Entity Framework Core, SQLite, LINQ