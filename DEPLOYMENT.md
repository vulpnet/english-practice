# Deployment Guide

## Render Web Service (Recommended)

### Prerequisites
- GitHub account with this repo pushed
- Render account (render.com)
- Existing Ollama server with public URL

### Steps

1. **Push to GitHub**
   ```bash
   git init
   git add .
   git commit -m "Initial commit"
   git remote add origin https://github.com/YOUR/english-practice.git
   git push -u origin main
   ```

2. **Create Render Web Service**
   - Go to render.com → New → Web Service
   - Connect your GitHub repo
   - Settings:
     - **Runtime:** Docker
     - **Docker Context:** `.` (repo root)
     - **Dockerfile:** `./Dockerfile`
     - **Plan:** Starter ($7/mo) or higher — avoid Free for production (cold starts)

3. **Set Environment Variables** (Render dashboard → Environment tab)
   ```
   JWT_SECRET=<generate: openssl rand -base64 32>
   JWT_ISSUER=english-practice-api
   JWT_AUDIENCE=english-practice-mobile

   # AI Backend — dùng Vercel chatbot (tools-chatbot.vercel.app) — không cần auth
   OLLAMA_BASE_URL=https://tools-chatbot.vercel.app
   OLLAMA_CHAT_ENDPOINT=/api/chat
   OLLAMA_API_FORMAT=vercel
   OLLAMA_TIMEOUT_SECONDS=30

   # Nếu muốn chuyển sang Ollama self-hosted sau này:
   # OLLAMA_BASE_URL=https://your-ollama-server.com
   # OLLAMA_API_FORMAT=native   (hoặc openai)
   # OLLAMA_USERNAME=your_username
   # OLLAMA_PASSWORD=your_password
   # OLLAMA_MODEL=your_model_name
   # OLLAMA_TIMEOUT_SECONDS=90
   ```

4. **Deploy** — Render auto-deploys on `git push origin main`

### Verify Deployment

```bash
BASE=https://your-app.onrender.com

# 1. Health
curl $BASE/health

# 2. Ollama connectivity
curl $BASE/health/ollama

# 3. Register
curl -X POST $BASE/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"username":"demo","email":"demo@example.com","password":"Password123!"}'

# 4. Login (save token)
TOKEN=$(curl -s -X POST $BASE/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"demo","password":"Password123!"}' \
  | jq -r '.data.accessToken')

# 5. Send a message
curl -X POST $BASE/api/conversations/test-conv-1/messages \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"message":"I go to restaurant yesterday.","scenario":"Restaurant","level":"Beginner","history":[]}'
```

## Mobile Configuration

After deploying, update the backend URL in the mobile app:

**Option A — Preferences (runtime, recommended for dev):**
```csharp
// In ApiService constructor, already reads from Preferences:
Preferences.Set("api_base_url", "https://your-app.onrender.com");
```

**Option B — Hardcode (for production build):**
Edit `src/EnglishPractice.Mobile/Services/ApiService.cs`:
```csharp
var baseUrl = "https://your-app.onrender.com";
```

## Android Build

```bash
# Debug APK
dotnet build src/EnglishPractice.Mobile -f net9.0-android -c Debug

# Release APK (requires keystore)
dotnet publish src/EnglishPractice.Mobile -f net9.0-android -c Release
```

APK output: `src/EnglishPractice.Mobile/bin/Release/net9.0-android/`

**Required permissions** (auto-added by MAUI):
- `RECORD_AUDIO` — microphone for STT
- `INTERNET` — backend communication

## iOS Build (macOS only)

```bash
dotnet build src/EnglishPractice.Mobile -f net9.0-ios -c Release
```

Requires Xcode + Apple Developer account for device deployment.

## Troubleshooting

| Problem | Check |
|---------|-------|
| `502 Bad Gateway` from Render | Check Render logs; app may still be starting |
| `/health/ollama` returns Unhealthy | Verify `OLLAMA_BASE_URL` is reachable from Render's IPs |
| `401 Unauthorized` on message endpoint | JWT token expired — call `/api/auth/refresh` |
| STT not working on Android | Confirm `RECORD_AUDIO` permission granted; device has Google STT service |
| AI returns non-JSON | Model may not follow instructions — check `OLLAMA_MODEL` name; test with `curl` |

## AI Backend Formats

| Format | Endpoint | Auth | Use case |
|--------|----------|------|----------|
| `vercel` | `/api/chat` | None | tools-chatbot.vercel.app (mặc định) |
| `native` | `/api/chat` | Basic Auth | Ollama self-hosted |
| `openai` | `/v1/chat/completions` | Basic Auth hoặc Bearer | OpenAI-compatible (Groq, etc.) |

Restart Render service sau khi đổi env vars.
