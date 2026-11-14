# English Practice — AI Speaking Coach

AI-powered English conversation practice app. Speak → AI corrects → listen back.

**Stack:** .NET MAUI (Android/iOS) + ASP.NET Core 9 Backend + Ollama LLM

## Architecture

```
Mobile (MAUI) ──JWT──> Backend (Render/Docker) ──BasicAuth──> Ollama Server
     STT/TTS on-device                               your existing server
```

## Quick Start

### Backend (local dev)

```bash
cd src/EnglishPractice.API
# Set env vars (or edit appsettings.Development.json)
export JWT_SECRET="your-secret-at-least-32-chars!!"
export OLLAMA_BASE_URL="http://localhost:11434"
export OLLAMA_MODEL="llama3"
dotnet run
# API: http://localhost:8080
# Swagger: http://localhost:8080/swagger
```

### Run tests

```bash
dotnet test tests/EnglishPractice.UnitTests/
```

### Docker build (local verify)

```bash
docker build -t english-practice-api .
docker run -p 8080:8080 \
  -e JWT_SECRET="your-secret-at-least-32-chars!!" \
  -e OLLAMA_BASE_URL="https://your-ollama.com" \
  -e OLLAMA_USERNAME="user" \
  -e OLLAMA_PASSWORD="pass" \
  -e OLLAMA_MODEL="llama3" \
  english-practice-api
```

### Mobile (Android)

```bash
cd src/EnglishPractice.Mobile
dotnet build -f net9.0-android
# Or open in Visual Studio 2022 / Rider
```

Before building, set your backend URL in `Services/ApiService.cs`:
```csharp
var baseUrl = Preferences.Get("api_base_url", "https://your-app.onrender.com");
```

## API Reference

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/auth/register` | — | Create account |
| POST | `/api/auth/login` | — | Login, get JWT |
| POST | `/api/auth/refresh` | — | Refresh access token |
| POST | `/api/conversations/{id}/messages` | JWT | Send voice transcript, get AI response |
| GET | `/health` | — | Backend health |
| GET | `/health/ollama` | — | Ollama connectivity |

### Send Message

```json
POST /api/conversations/{id}/messages
Authorization: Bearer <token>

{
  "message": "I go to restaurant yesterday.",
  "scenario": "Restaurant",
  "level": "Beginner",
  "history": [
    {"role": "assistant", "content": "Welcome! What would you like to practice?"}
  ]
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "reply": "Oh, did you enjoy the food?",
    "originalText": "I go to restaurant yesterday.",
    "correctedText": "I went to the restaurant yesterday.",
    "explanation": "Dùng 'went' (quá khứ) thay vì 'go' khi nói về hành động đã xảy ra.",
    "naturalExpression": "I went out to eat yesterday."
  }
}
```

## Ollama Configuration

| Env Var | Description | Example |
|---------|-------------|---------|
| `OLLAMA_BASE_URL` | Server URL | `https://ai.company.com` |
| `OLLAMA_CHAT_ENDPOINT` | API path | `/api/chat` |
| `OLLAMA_API_FORMAT` | `native` or `openai` | `native` |
| `OLLAMA_USERNAME` | Basic auth user | `myuser` |
| `OLLAMA_PASSWORD` | Basic auth pass | `mypass` |
| `OLLAMA_MODEL` | Model name | `llama3` |
| `OLLAMA_TIMEOUT_SECONDS` | Request timeout | `90` |

**Switch to OpenAI-compatible API:**
```
OLLAMA_API_FORMAT=openai
OLLAMA_CHAT_ENDPOINT=/v1/chat/completions
```

## Security Notes

- Backend credentials (Ollama Basic Auth) never reach mobile
- JWT expires in 15 min; refresh token valid 7 days
- Credentials loaded from env vars only — never hardcoded
- SSL required for production Ollama connections
- Rate limit: 60 req/min per IP

## Project Structure

```
src/
  EnglishPractice.Domain/        — Entities, Enums
  EnglishPractice.Application/   — Interfaces, DTOs
  EnglishPractice.Infrastructure/ — OllamaClient, Auth, Services
  EnglishPractice.API/           — Controllers, Middleware, Health
  EnglishPractice.Mobile/        — MAUI App (Android + iOS)
tests/
  EnglishPractice.UnitTests/     — Service unit tests (xUnit + Moq)
  EnglishPractice.IntegrationTests/ — API integration tests
```

See [DEPLOYMENT.md](DEPLOYMENT.md) for Render deployment guide.
