# Hướng dẫn Build iOS/Android qua GitHub Actions

## Bước 1 — Push code lên GitHub

```bash
cd "D:/Jira comment/tools/EnglishPractice"

git init
git add .
git commit -m "Initial commit: English Practice app"

# Tạo repo mới trên github.com rồi:
git remote add origin https://github.com/YOUR_USERNAME/english-practice.git
git branch -M main
git push -u origin main
```

## Bước 2 — Xem kết quả build

Sau khi push, GitHub tự động chạy 2 workflow:

1. Vào repo → tab **Actions**
2. Thấy 2 jobs đang chạy:
   - `iOS Build` — chạy trên macOS runner (~10-15 phút)
   - `Android Build` — chạy trên Ubuntu runner (~5-8 phút)
3. Khi xong → click vào job → kéo xuống **Artifacts**
4. Download file về:
   - `EnglishPractice-iOS-Simulator.zip` → chứa `.app` chạy trên iOS Simulator
   - `EnglishPractice-Android-Debug-APK.zip` → chứa `.apk` cài trực tiếp trên Android

---

## Dùng file iOS Simulator (.app)

File `.app` chạy được trên **Xcode iOS Simulator** (cần máy Mac):

```bash
# Mở Simulator
open -a Simulator

# Kéo thả file .app vào cửa sổ Simulator
# HOẶC:
xcrun simctl install booted EnglishPractice.app
xcrun simctl launch booted com.yourcompany.englishpractice
```

> File `.app` từ simulator **không cài được lên iPhone thật**.
> Muốn cài lên iPhone thật → cần IPA (xem bên dưới).

---

## Build IPA thật để cài trên iPhone (cần Apple Developer)

### Cần có:
- Apple Developer account ($99/năm) tại developer.apple.com
- Distribution Certificate (.p12)
- Provisioning Profile (.mobileprovision)

### Bước setup GitHub Secrets:

Vào repo → **Settings → Secrets and variables → Actions → New repository secret**

| Secret name | Giá trị |
|-------------|---------|
| `APPLE_CERTIFICATE_BASE64` | `base64 -i YourCert.p12` |
| `APPLE_CERTIFICATE_PASSWORD` | Password của file .p12 |
| `APPLE_PROVISIONING_PROFILE_BASE64` | `base64 -i YourProfile.mobileprovision` |
| `APPLE_CODESIGN_KEY` | Tên cert, ví dụ: `iPhone Distribution: Your Name (XXXXXXXXXX)` |
| `APPLE_PROVISIONING_PROFILE_NAME` | Tên profile trong Apple Developer portal |

### Chạy build IPA:

Vào **Actions → iOS Build → Run workflow** (chọn branch main)

> Job `build-ipa` chỉ chạy khi trigger thủ công **và** có đủ secrets.

---

## Cài APK lên Android (không cần developer account)

```bash
# Enable "Unknown sources" trên Android:
# Settings → Security → Install unknown apps → Files → Allow

# Kết nối USB + ADB:
adb install EnglishPractice.apk
```

Hoặc copy file .apk vào điện thoại và mở trực tiếp.

---

## Troubleshooting

| Lỗi | Giải pháp |
|-----|-----------|
| `MAUI workload not found` | Runner chưa có workload — workflow đã tự install, thử chạy lại |
| `Code signing failed` | Kiểm tra secrets đã đúng tên và format base64 chưa |
| `Bundle ID mismatch` | Sửa `ApplicationId` trong `.csproj` trùng với Provisioning Profile |
| iOS build timeout | macOS runner mất ~15 phút, bình thường |
