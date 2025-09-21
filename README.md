# 🗓️ CalendarApi
Projeto de exemplo: **API de calendário** em ASP.NET Core + EF Core (SQLite) 🖥️

Este projeto tem como objetivo integrar com o **app do investidor**, permitindo listar, criar e gerenciar eventos no calendário do usuário de forma prática e centralizada.
---
## DWE - Team
- Deivison Pertel – RM 550803
- Eduardo Akira Murata – RM 98713
- Wesley Souza de Oliveira – RM 97874


## 🚀 Como rodar

1. Restaurar pacotes:
   ```bash
   dotnet restore
   ```
2. (Opcional) Criar migração e aplicar banco:
   ```bash
   dotnet tool install --global dotnet-ef  # se ainda não tiver
   dotnet ef migrations add Initial -p src/CalendarApi -s src/CalendarApi
   dotnet ef database update -p src/CalendarApi -s src/CalendarApi
   ```
3. Rodar a API:
   ```bash
   dotnet run --project src/CalendarApi
   ```
4. Abrir Swagger para testar endpoints:
   ```
https://localhost:5001/swagger
```

## 📌 Endpoints principais

- `GET /api/events?email={email}` → Listar eventos por e-mail
- `POST /api/events` → Criar evento (body: `EventCreateDto`) ✏️
- `PUT /api/events/{id}` → Atualizar evento 🔄
- `DELETE /api/events/{id}` → Deletar evento ❌
- `GET /api/events/export?email={email}` → Exportar eventos em JSON 📤
- `POST /api/events/import` → Importar eventos em JSON 📥 (body: `{ data: [ ... ] }`)

## ⚙️ Observações

- Campos de data/hora mapeados para **DateTime/TimeSpan**, não strings ⏰
- Banco padrão: **SQLite** (`calendar.db`) 💾
- Projetado para integração com front-end do investidor, facilitando sincronização de eventos no calendário do usuário.