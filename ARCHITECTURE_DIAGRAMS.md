# 🏗️ Diagramas de Arquitetura - CalendarApi Sprint 4

## 1. Arquitetura Geral do Sistema

```mermaid
graph TB
    Client[Cliente/Frontend] --> API[ASP.NET Core Web API]
    API --> Controller[Controllers Layer]
    Controller --> Service[Services Layer]
    Service --> Repository[Entity Framework Core]
    Repository --> Database[(SQLite Database)]
    
    subgraph "API Layer"
        Controller --> EventsController[EventsController]
        Controller --> ReportsController[ReportsController]
    end
    
    subgraph "Business Layer"
        Service --> EventService[EventService]
        Service --> IEventService[IEventService Interface]
    end
    
    subgraph "Data Layer"
        Repository --> CalendarDbContext[CalendarDbContext]
        Repository --> EventItem[EventItem Entity]
    end
    
    subgraph "DTOs"
        DTO1[EventCreateDto]
        DTO2[EventUpdateDto]
        DTO3[EventResponseDto]
        DTO4[EventSearchDto]
    end
    
    Controller -.-> DTO1
    Controller -.-> DTO2
    Controller -.-> DTO3
    Controller -.-> DTO4
```

## 2. Fluxo de Dados - CRUD Operations

```mermaid
sequenceDiagram
    participant C as Cliente
    participant API as Web API
    participant Ctrl as Controller
    participant Svc as Service
    participant DB as Database
    
    Note over C,DB: CREATE Event
    C->>API: POST /api/events
    API->>Ctrl: EventsController.Create()
    Ctrl->>Ctrl: Validate ModelState
    Ctrl->>Svc: EventService.CreateAsync()
    Svc->>Svc: Map DTO to Entity
    Svc->>DB: _context.Events.Add()
    DB-->>Svc: Entity Created
    Svc-->>Ctrl: EventResponseDto
    Ctrl-->>API: 201 Created
    API-->>C: Event Created
    
    Note over C,DB: READ Event
    C->>API: GET /api/events/{id}
    API->>Ctrl: EventsController.GetById()
    Ctrl->>Svc: EventService.GetByIdAsync()
    Svc->>DB: _context.Events.FindAsync()
    DB-->>Svc: EventItem
    Svc-->>Ctrl: EventResponseDto
    Ctrl-->>API: 200 OK
    API-->>C: Event Data
    
    Note over C,DB: UPDATE Event
    C->>API: PUT /api/events/{id}
    API->>Ctrl: EventsController.Update()
    Ctrl->>Ctrl: Validate ModelState
    Ctrl->>Svc: EventService.UpdateAsync()
    Svc->>DB: Update Entity
    DB-->>Svc: Entity Updated
    Svc-->>Ctrl: EventResponseDto
    Ctrl-->>API: 200 OK
    API-->>C: Updated Event
    
    Note over C,DB: DELETE Event
    C->>API: DELETE /api/events/{id}
    API->>Ctrl: EventsController.Delete()
    Ctrl->>Svc: EventService.DeleteAsync()
    Svc->>DB: Remove Entity
    DB-->>Svc: Entity Removed
    Svc-->>Ctrl: Boolean Success
    Ctrl-->>API: 204 No Content
    API-->>C: Success
```

## 3. Arquitetura de Camadas (Layered Architecture)

```mermaid
graph TD
    subgraph "Presentation Layer"
        A[Swagger UI] --> B[HTTP Requests/Responses]
        B --> C[Controllers]
    end
    
    subgraph "Application Layer"
        C --> D[DTOs]
        C --> E[Services]
        E --> F[Business Logic]
    end
    
    subgraph "Data Access Layer"
        F --> G[Entity Framework Core]
        G --> H[DbContext]
        H --> I[Repositories Pattern]
    end
    
    subgraph "Database Layer"
        I --> J[(SQLite Database)]
        J --> K[EventItem Table]
    end
    
    subgraph "Cross-Cutting Concerns"
        L[Validation]
        M[Error Handling]
        N[Logging]
        O[Dependency Injection]
    end
    
    C -.-> L
    C -.-> M
    E -.-> N
    E -.-> O
```

## 4. Padrões de Design Implementados

```mermaid
classDiagram
    class IEventService {
        <<interface>>
        +GetByIdAsync(id) EventResponseDto
        +GetByEmailAsync(email) IEnumerable~EventResponseDto~
        +SearchEventsAsync(searchDto) PagedResult~EventResponseDto~
        +CreateAsync(createDto) EventResponseDto
        +UpdateAsync(id, updateDto) EventResponseDto
        +DeleteAsync(id) bool
    }
    
    class EventService {
        -_context: CalendarDbContext
        +GetByIdAsync(id) EventResponseDto
        +GetByEmailAsync(email) IEnumerable~EventResponseDto~
        +SearchEventsAsync(searchDto) PagedResult~EventResponseDto~
        +CreateAsync(createDto) EventResponseDto
        +UpdateAsync(id, updateDto) EventResponseDto
        +DeleteAsync(id) bool
        -MapToResponseDto(eventItem) EventResponseDto
    }
    
    class EventsController {
        -_eventService: IEventService
        -_db: CalendarDbContext
        +GetByEmail(email) IActionResult
        +GetById(id) IActionResult
        +Create(createDto) IActionResult
        +Update(id, updateDto) IActionResult
        +Delete(id) IActionResult
        +SearchEvents(searchDto) IActionResult
    }
    
    class EventItem {
        +Id: int
        +Title: string
        +Date: DateTime
        +Time: TimeSpan
        +Client: string
        +Type: string
        +ReminderMinutes: int
        +Notes: string
        +Email: string
        +CreatedAt: DateTime
        +UpdatedAt: DateTime
    }
    
    class EventCreateDto {
        +Title: string
        +Date: DateTime
        +Time: TimeSpan
        +Client: string
        +Type: string
        +ReminderMinutes: int
        +Notes: string
        +Email: string
    }
    
    class EventResponseDto {
        +Id: int
        +Title: string
        +Date: DateTime
        +Time: TimeSpan
        +Client: string
        +Type: string
        +ReminderMinutes: int
        +Notes: string
        +Email: string
        +CreatedAt: DateTime
        +UpdatedAt: DateTime
    }
    
    IEventService <|.. EventService : implements
    EventsController --> IEventService : uses
    EventService --> EventItem : maps to/from
    EventsController --> EventCreateDto : receives
    EventsController --> EventResponseDto : returns
```

## 5. Fluxo de Pesquisas LINQ Avançadas

```mermaid
flowchart TD
    A[Search Request] --> B[EventSearchDto Validation]
    B --> C[EventService.SearchEventsAsync]
    C --> D[Build Base Query]
    D --> E{Apply Filters}
    
    E --> F[Date Range Filter]
    E --> G[Type Filter]
    E --> H[Client Filter]
    E --> I[Search Term Filter]
    
    F --> J[Apply Sorting]
    G --> J
    H --> J
    I --> J
    
    J --> K[Apply Pagination]
    K --> L[Execute Query]
    L --> M[Map to DTOs]
    M --> N[Return PagedResult]
    
    subgraph "LINQ Operations"
        O[Where Clauses]
        P[GroupBy Operations]
        Q[OrderBy/ThenBy]
        R[Skip/Take Pagination]
        S[Select Projections]
    end
    
    D -.-> O
    J -.-> Q
    K -.-> R
    L -.-> S
```

## 6. Estrutura de Banco de Dados

```mermaid
erDiagram
    EventItem {
        int Id PK
        string Title
        DateTime Date
        TimeSpan Time
        string Client
        string Type
        int ReminderMinutes
        string Notes
        string Email
        DateTime CreatedAt
        DateTime UpdatedAt
    }
    
    EventItem ||--o{ EventItem : "Email Index"
```

## 7. Fluxo de Tratamento de Erros

```mermaid
flowchart TD
    A[Request Received] --> B[Controller Action]
    B --> C{Model Validation}
    C -->|Invalid| D[Return 400 Bad Request]
    C -->|Valid| E[Service Call]
    E --> F{Service Operation}
    F -->|Success| G[Return Success Response]
    F -->|Not Found| H[Return 404 Not Found]
    F -->|Exception| I[Try-Catch Block]
    I --> J[Log Error]
    J --> K[Return 500 Internal Server Error]
    
    subgraph "Error Response Format"
        L[Error Object]
        M[Error Message]
        N[Error Details - Dev Only]
    end
    
    D --> L
    H --> L
    K --> L
```

## 8. Injeção de Dependência

```mermaid
graph LR
    A[Program.cs] --> B[Service Registration]
    B --> C[IEventService → EventService]
    B --> D[CalendarDbContext]
    B --> E[Controllers]
    
    E --> F[EventsController]
    E --> G[ReportsController]
    
    F --> H[IEventService Injected]
    F --> I[CalendarDbContext Injected]
    
    G --> I
    
    subgraph "Service Lifetime"
        J[Scoped: EventService]
        K[Scoped: CalendarDbContext]
        L[Singleton: Configuration]
    end
    
    C --> J
    D --> K
```

## 9. Endpoints e Rotas

```mermaid
graph TD
    A[API Base: /api] --> B[Events Controller]
    A --> C[Reports Controller]
    
    B --> D[GET /events?email]
    B --> E[GET /events/{id}]
    B --> F[POST /events]
    B --> G[PUT /events/{id}]
    B --> H[DELETE /events/{id}]
    B --> I[POST /events/search]
    B --> J[GET /events/upcoming]
    B --> K[GET /events/by-type]
    B --> L[GET /events/by-client]
    B --> M[GET /events/statistics]
    B --> N[GET /events/export]
    B --> O[POST /events/import]
    
    C --> P[GET /reports/events-by-period]
    C --> Q[GET /reports/client-productivity]
    C --> R[GET /reports/temporal-trends]
    C --> S[GET /reports/time-conflicts]
```

## 10. Tecnologias e Dependências

```mermaid
graph TB
    A[CalendarApi Project] --> B[ASP.NET Core 8.0]
    A --> C[Entity Framework Core 7.0]
    A --> D[SQLite Database]
    A --> E[Swagger/OpenAPI]
    
    B --> F[Microsoft.AspNetCore.Mvc]
    B --> G[Microsoft.AspNetCore.OpenApi]
    
    C --> H[Microsoft.EntityFrameworkCore.Sqlite]
    C --> I[Microsoft.EntityFrameworkCore.Tools]
    
    E --> J[Swashbuckle.AspNetCore]
    
    subgraph "Development Tools"
        K[dotnet CLI]
        L[Entity Framework CLI]
        M[Visual Studio/VS Code]
    end
    
    A -.-> K
    A -.-> L
    A -.-> M
```

---

**Nota:** Estes diagramas representam a arquitetura implementada na Sprint 4, demonstrando os padrões de design, fluxos de dados e estruturas utilizadas no projeto CalendarApi.
