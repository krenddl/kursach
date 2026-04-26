# DigiClinic Deployment Notes

This project can work in two modes:

1. Local mode: React runs on Vite, API runs on ASP.NET Core, PostgreSQL is local.
2. Hosted mode: React is hosted separately, API runs from the Dockerfile, PostgreSQL is a hosted database.

## Local

API:

```powershell
cd DigiClinicApi/DigiClinicApi
dotnet run
```

Client:

```powershell
cd digiclinic/digi-clinic-client
npm install
npm run dev
```

Local API URL is already the default for the client:

```text
http://localhost:5237/api
```

Health check:

```text
http://localhost:5237/api/health
```

## Hosted API

The root `Dockerfile` builds only the ASP.NET Core API.

Set these environment variables on Render/Railway or another host:

```text
ASPNETCORE_ENVIRONMENT=Production
PORT=8080
ConnectionStrings__DefaultConnection=Host=...;Port=5432;Database=...;Username=...;Password=...;SSL Mode=Require;Trust Server Certificate=true
Jwt__Key=change-this-to-a-long-random-secret-at-least-32-characters
Jwt__Issuer=DigiClinic.Api
Jwt__Audience=DigiClinic.Client
Jwt__ExpiresInMinutes=120
Frontend__AllowedOrigins__0=https://your-frontend.vercel.app
```

The API applies EF Core migrations and seed data on startup, so the database must be reachable before the service starts.

## Telegram Bot

The Telegram bot is part of the API and is disabled by default.

For local testing without hosting, enable long polling:

```text
Telegram__Enabled=true
Telegram__BotToken=your-telegram-bot-token
Telegram__PollingTimeoutSeconds=25
```

Then run the API normally. The bot works while the API process is running.

Patient flow:

```text
1. Open profile in the web app.
2. Click "Получить код" in the Telegram bot card.
3. Send the generated /link 123456 command to the bot.
4. Use /appointments, /doctors, /services, /slots, /book, /cancel.
```

If the API is hosted later, the same long polling mode can still work. A webhook can be added later, but it is not required for the course demo.

## Hosted Client

For Vercel or another static host, set:

```text
VITE_API_BASE_URL=https://your-api-host.com/api
```

Then build:

```powershell
npm run build
```

SignalR uses the same API base URL, so chat and appointment updates will point to the hosted API automatically.
