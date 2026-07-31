# 01 — Architecture

**RB-026** · Evidence date: 2026-07-30 · Baseline commit before hardening: `202eb9a`

## Detected architecture

| Layer | Technology | Evidence |
|-------|------------|----------|
| Web | ASP.NET Core 8 MVC + API | `RestBar.csproj`, Controllers |
| Realtime | SignalR `OrderHub` `/orderHub` | `Hubs/OrderHub.cs` |
| Data | EF Core 9 + Npgsql / PostgreSQL 15 | `Program.cs`, docker-compose |
| Auth | Cookie `RestBarAuth` + role policies | `Program.cs` |
| Multitenancy | Company → Branch claims + middleware | `TenantSubscriptionMiddleware` |
| Feature flags | `FeatureFlags` options | `Infrastructure/Foundation` |
| Deploy | Docker Compose isolated `restbar_*` :8084 | `docker-compose.yml` |

## Pattern

Monolith modular (enterprise modules Cash/Procurement/FoodCost/BI gated by flags). **No** separate API gateway, **no** message bus, **no** hosted background workers.

## Classification

| Item | Status |
|------|--------|
| Clear deployable unit | **PASS** |
| Separation of concerns (services/controllers) | **PASS WITH CONDITIONS** |
| Horizontal scale-ready (sticky sessions / shared DP keys) | **PASS WITH CONDITIONS** (DP keys volume added) |
| Microservices / multi-region | **NOT APPLICABLE** / not designed |

**Overall architecture:** **PASS WITH CONDITIONS**
