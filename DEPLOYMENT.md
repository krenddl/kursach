# DigiClinic Deployment Notes

DigiClinic can run locally and can also be hosted on Render without Vercel.

## Local mode

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

Default local URLs:

```text
API health: http://localhost:5237/api/health
Client:     http://localhost:5173
```

## Render mode

The root `render.yaml` creates three resources:

```text
digiclinic-api     ASP.NET Core API from Dockerfile
digiclinic-db      PostgreSQL database
digiclinic-client  React static site
```

Recommended flow:

```text
1. Push this repository to GitHub.
2. Open Render -> New -> Blueprint.
3. Connect the repository.
4. Render detects render.yaml and creates API, database, and client.
5. Check https://digiclinic-api.onrender.com/api/health.
6. Open https://digiclinic-client.onrender.com.
```

If Render gives the services different URLs, update these environment variables:

```text
digiclinic-client:
VITE_API_BASE_URL=https://your-api-url.onrender.com/api

digiclinic-api:
Frontend__AllowedOrigins__0=https://your-client-url.onrender.com
```

After changing `Frontend__AllowedOrigins__0`, redeploy or restart the API.

## Manual Render frontend setup

If you do not want to use Blueprint for the frontend, create a separate Render Static Site:

```text
Root Directory:    digiclinic/digi-clinic-client
Build Command:     npm ci && npm run build
Publish Directory: dist
```

Environment variable:

```text
VITE_API_BASE_URL=https://your-api-url.onrender.com/api
```

Then add the frontend URL to API CORS:

```text
Frontend__AllowedOrigins__0=https://your-client-url.onrender.com
```

## Telegram bot

The Telegram bot is part of the API and is disabled by default.

For local testing:

```text
Telegram__Enabled=true
Telegram__BotToken=your-telegram-bot-token
Telegram__PollingTimeoutSeconds=25
```

Then run the API normally. The bot works while the API process is running.

Patient flow:

```text
1. Open the patient profile in the web app.
2. Generate a Telegram link code.
3. Send /link 123456 to the bot.
4. Use /appointments, /doctors, /services, /slots, /book, /cancel.
```

For hosting, add the same Telegram variables to `digiclinic-api` on Render.

## Before commit

The repository currently has generated build files under `bin`, `obj`, and `dist`.
Do not stage them with `git add .`.

Stage only source and config files, for example:

If `node_modules`, `dist`, `bin`, or `obj` were committed earlier, remove them from Git tracking once:

```powershell
git rm -r --cached digiclinic/digi-clinic-client/node_modules digiclinic/digi-clinic-client/dist DigiClinicApi/DigiClinicApi/bin DigiClinicApi/DigiClinicApi/obj
```

Then commit only source/config changes.
