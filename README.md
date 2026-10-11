# QueueLess

A complete MVP queue-management system built with:

- Mobile: React Native + Expo + TypeScript
- API: ASP.NET Core Web API (.NET 10)
- Database: SQLite + Entity Framework Core
- Real-time: SignalR
- Auth: JWT bearer tokens
- Notifications: local notifications in the mobile MVP

## Features

Customer:
- Register/login
- Browse demo businesses and services
- Join a queue
- See ticket number, current number, people ahead and estimated wait
- Leave a queue
- Receive live SignalR updates

Staff:
- Login with the demo staff account
- View queue
- Call next customer
- Complete current customer
- Skip current customer
- Pause/resume queue

Demo accounts

Customer:
- Email: customer@queueless.local
- Password: Password123!

Staff:
- Email: staff@queueless.local
- Password: Password123!

## Project structure

QueueLess/
  backend/QueueLess.Api
  mobile

## Run backend

Prerequisites:
- .NET 10 SDK

From backend/QueueLess.Api:

    dotnet restore
    dotnet run

The API runs on:
    http://localhost:5077

Swagger:
    http://localhost:5077/swagger

SignalR:
    http://localhost:5077/hubs/queue

## Run mobile

Prerequisites:
- Node.js 20+
- npm
- Expo CLI through `npx expo`
- Android Studio/emulator OR a physical Android phone with Expo Go

From mobile:

    npm install
    npx expo start

### API URL

Open `src/config.ts`.

For a physical Android phone, replace API_BASE_URL with your computer's LAN IP:

    http://192.168.1.10:5077

Your phone and computer must be on the same network.

For an Android emulator use:

    http://10.0.2.2:5077

## Production packaging

Backend:
    dotnet publish -c Release -o publish

Then deploy the publish folder to an ASP.NET-compatible host and set:
- JWT secret
- database connection string
- allowed origins
- HTTPS

Mobile Android:
    npm install
    npx expo install expo-dev-client
    npx eas-cli@latest login
    npx eas-cli@latest build:configure
    npx eas-cli@latest build --platform android --profile preview

For a Play Store AAB:
    npx eas-cli@latest build --platform android --profile production

See BUILD.md for detailed packaging guidance.

## Important production changes

This repository is intentionally self-contained and easy to run. Before public deployment:
- replace the demo JWT secret
- move secrets to environment variables / secret storage
- use PostgreSQL instead of SQLite for multi-instance production
- use HTTPS
- add refresh tokens
- add server-side authorization policies
- use push notifications such as Expo Notifications
- add rate limiting
- validate uploaded media if uploads are added
- configure CORS for the actual mobile/web clients
- add logging and health checks
