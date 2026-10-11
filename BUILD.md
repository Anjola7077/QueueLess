# QueueLess build and packaging guide

## 1. Backend development

Install .NET 10 SDK.

From backend/QueueLess.Api:

    dotnet restore
    dotnet run

Check:
    http://localhost:5077/swagger

The SQLite database is created automatically as `queueless.db`.

## 2. Mobile development

Install Node.js 20+.

From mobile:

    npm install
    npx expo start

For Android:
- press `a` for an emulator
- or scan the Expo QR code with Expo Go

## 3. Physical Android phone

A physical phone cannot use `localhost` to reach your PC.

Find your PC's LAN IP.

Windows PowerShell:
    ipconfig

Find the IPv4 address, for example:
    192.168.1.10

Set:
    API_BASE_URL = "http://192.168.1.10:5077"

Also allow port 5077 through Windows Firewall if prompted.

## 4. Android emulator

Use:
    http://10.0.2.2:5077

This maps the emulator's localhost to the host machine.

## 5. Production API

Recommended deployment:

Client
  |
 HTTPS
  |
 ASP.NET Core
  |
 PostgreSQL

Publish:

    dotnet publish -c Release -o publish

For Linux hosting:

    dotnet publish -c Release -r linux-x64 --self-contained false -o publish

Use a reverse proxy such as Nginx/Caddy and HTTPS.

Environment variables should contain:
- ConnectionStrings__Default
- Jwt__Key
- Jwt__Issuer
- Jwt__Audience

Never commit production secrets.

## 6. Database

SQLite is ideal for local development and a single small server.

For production, switch EF Core provider to PostgreSQL:

    Npgsql.EntityFrameworkCore.PostgreSQL

Then configure:

    UseNpgsql(connectionString)

Run migrations:

    dotnet ef migrations add InitialCreate
    dotnet ef database update

## 7. Android APK

Install EAS:

    npm install -g eas-cli

Login:

    eas login

Configure:

    eas build:configure

Preview APK:

    eas build --platform android --profile preview

The generated APK can be installed directly on Android.

## 8. Google Play AAB

Use:

    eas build --platform android --profile production

EAS produces an Android App Bundle (AAB).

Then upload it through Google Play Console.

## 9. App icon and splash

Replace:
- mobile/assets/icon.png
- mobile/assets/adaptive-icon.png
- mobile/assets/splash.png

Use a square icon with safe padding.

## 10. Release checklist

Backend:
[ ] HTTPS
[ ] production database
[ ] strong JWT secret
[ ] restricted CORS
[ ] logging
[ ] health endpoint
[ ] backups
[ ] rate limiting

Mobile:
[ ] production API URL
[ ] app name
[ ] icon
[ ] splash
[ ] version number
[ ] Android package name
[ ] notification configuration
[ ] privacy policy URL

Queue logic:
[ ] transaction/locking strategy
[ ] prevent duplicate active tickets
[ ] staff authorization
[ ] audit logging
[ ] queue pause/resume rules
