# 12 — Integration Analysis

**Sistema:** RestBar  
**Fecha:** 2026-07-04

---

## 1. Resumen de Integraciones

| Tipo | Cantidad | Estado |
|------|----------|--------|
| Integraciones externas | 1 (SMTP) | Opcional, deshabilitada por defecto |
| Integraciones internas | 5 subsistemas | Activas |
| APIs externas consumidas | 0 | — |
| Webhooks | 0 | — |
| Pasarelas de pago | 0 | Pagos internos POS |
| Servicios de impresión | 0 | No implementado |

---

## 2. Integraciones Externas

### 2.1 Email (SMTP via MailKit)

| Aspecto | Detalle |
|---------|---------|
| **Proveedor** | Configurable (default: Gmail SMTP) |
| **Protocolo** | SMTP con StartTLS |
| **Host default** | smtp.gmail.com:587 |
| **Servicio** | `EmailService` (MailKit + MimeKit) |
| **Controller** | `EmailController` (SystemConfig) |
| **Habilitación** | `Email:Enabled` en appsettings (default: false) |
| **Templates** | Almacenados en DB (`email_templates`) |

#### Casos de Uso

| Trigger | Template | Condición |
|---------|----------|-----------|
| Pago completo | OrderConfirmation | Email:Enabled + NotificationSettings.OrderConfirmation |
| Recuperación contraseña | PasswordRecovery | Email:Enabled |
| Bienvenida usuario | Welcome | Manual/admin |
| Prueba SMTP | — | EmailController.TestConnection |

#### Flujo de Integración

```
Evento de negocio (ej: pago completo)
  ↓
PaymentService / AuthService
  ↓
EmailService.Send*Async()
  ├── Verificar Email:Enabled
  ├── Verificar NotificationSettings
  ├── Cargar template de DB
  ├── Renderizar con datos
  └── MailKit: Connect → Auth → Send → Disconnect
  ↓
Si falla → catch (no bloquea operación principal)
```

### 2.2 Integraciones NO Encontradas

| Integración | Búsqueda realizada | Resultado |
|------------|-------------------|-----------|
| Stripe/PayPal/MercadoPago | Payment controllers, services | No integrado |
| APIs fiscales/e-invoicing | Invoice service | Solo entidad DB interna |
| SMS/WhatsApp | Codebase completo | No integrado |
| Cloud storage (S3/GCS) | Codebase | No integrado |
| Servicios de impresión | AdvancedSettings.Printers | Vista sin backend |
| n8n (mismo VPS) | Codebase RestBar | Sin integración code-level |
| Google Maps / Geolocation | Codebase | No integrado |
| Servicios de backup cloud | BackupSettingsService | Simulado (Task.Delay) |

---

## 3. Integraciones Internas

### 3.1 PostgreSQL (Base de Datos)

| Aspecto | Detalle |
|---------|---------|
| **Driver** | Npgsql 9.0.4 |
| **ORM** | EF Core 9.0.5 |
| **Conexión** | Connection string via config/env |
| **Migraciones** | Auto-aplicadas en producción al startup |
| **Enums** | UserRole mapeado como PG enum |
| **JSON** | jsonb para audit logs, user assignments |

### 3.2 SignalR (Tiempo Real)

| Aspecto | Detalle |
|---------|---------|
| **Hub** | OrderHub en `/orderHub` |
| **Servicio servidor** | OrderHubService |
| **Clientes** | POS (Order/Index), KDS (StationOrders), Stock updates |
| **Protocolo** | WebSocket (con fallback) |
| **Proxy** | nginx con Upgrade headers |

#### Mapa de Eventos Internos

```
OrderService ──→ OrderHubService ──→ OrderHub ──→ Clientes
PaymentService ──→     ↑
KitchenService ──→     ↑
TableService ──→       ↑
```

| Evento | Productor | Consumidor |
|--------|----------|-----------|
| OrderStatusChanged | OrderService | POS, KDS |
| OrderItemStatusChanged | OrderService, KitchenService | POS, KDS |
| TableStatusChanged | OrderService, TableService | POS (tables.js) |
| KitchenUpdate | KitchenService | KDS |
| PaymentProcessed | PaymentService | POS (payments.js) |
| NewOrder | OrderService | KDS |
| OrderCompleted | PaymentService | POS, KDS |
| OrderCancelled | OrderService | POS, KDS |

### 3.3 Sistema de Auditoría

| Aspecto | Detalle |
|---------|---------|
| **Middleware** | AuditMiddleware (cada request) |
| **Servicio** | AuditLogService |
| **Storage** | PostgreSQL `audit_logs` |
| **Error tracking** | ErrorHandlingMiddleware → AuditLogService |

### 3.4 Sistema de Notificaciones In-App

| Aspecto | Detalle |
|---------|---------|
| **Entidad** | `notifications` table |
| **Servicio** | NotificationService |
| **Uso en controllers** | Limitado (mayormente via SignalR) |

### 3.5 API Interna (JSON Endpoints)

El sistema expone ~100+ endpoints JSON via MVC controllers (no REST puro):

| Estilo | Controllers | Formato |
|--------|------------|---------|
| MVC + JSON | Order, Product, Table, User, etc. | `return Json(...)` |
| API REST | PaymentController, KitchenApiController | ControllerBase + route attributes |

---

## 4. Integración Frontend ↔ Backend

### 4.1 Patrón de Comunicación

```
Browser (Razor + JS)
  ├── Page load → MVC Controller → View (HTML)
  ├── User action → fetch() → MVC/API Controller → JSON
  ├── Real-time ← SignalR Hub ← OrderHubService
  └── Form submit → POST → MVC Controller (con CSRF en algunos)
```

### 4.2 Dependencias CDN

| CDN | Recurso | Impacto si cae |
|-----|---------|---------------|
| cdn.datatables.net | DataTables | Tablas admin sin funcionalidad |
| cdn.jsdelivr.net | SweetAlert2 | Sin alertas/modales |
| cdnjs.cloudflare.com | Font Awesome, SignalR | Sin iconos / sin real-time |
| fonts.googleapis.com | Inter font | Fallback a system font |
| unpkg.com | SignalR (KDS) | KDS sin real-time |

---

## 5. Integración de Despliegue

### 5.1 Docker Compose

```
docker-compose.yml
  ├── restbar_web (ASP.NET 8)
  │     depends_on: restbar_postgres (healthy)
  │     volumes: dataprotection-keys
  │     networks: restbar_net
  └── restbar_postgres (PostgreSQL 15)
        volumes: postgres_data
        networks: restbar_net
```

### 5.2 nginx → Docker

```
Internet → nginx:443 (SSL)
  → proxy_pass http://127.0.0.1:8084
  → WebSocket upgrade para /orderHub
  → Headers: X-Forwarded-For, X-Forwarded-Proto
```

### 5.3 VPS Multi-App

RestBar coexiste en el mismo VPS con otras aplicaciones:

| App | Puerto | Dominio |
|-----|--------|---------|
| CarnetQR | 80 | carnet.autonomousflow.lat |
| FixHub | 8081 | fixhub.autonomousflow.lat |
| PanamaTravelHub | 8082 | travel.autonomousflow.lat |
| n8n | 8083 | n8n.autonomousflow.lat |
| **RestBar** | **8084** | **restbar.autonomousflow.lat** |

**Aislamiento:** Contenedores, redes y volúmenes con prefijo `restbar_*`.

---

## 6. Diagrama de Integraciones

```mermaid
flowchart LR
    subgraph External
        SMTP[smtp.gmail.com:587]
    end

    subgraph Browser
        POS[POS View]
        KDS[KDS View]
        Admin[Admin Views]
    end

    subgraph RestBar App
        MVC[MVC Controllers]
        API[API Controllers]
        Svc[Services]
        Hub[OrderHub]
        Email[EmailService]
    end

    subgraph Data
        PG[(PostgreSQL)]
    end

    subgraph Infra
        nginx[nginx SSL]
        Docker[Docker]
    end

    POS & KDS & Admin --> MVC & API
    MVC & API --> Svc
    Svc --> PG
    Svc --> Hub
    Svc --> Email
    Email -.->|optional| SMTP
    POS & KDS <--> Hub
    nginx --> Docker
    Docker --> RestBar App
```

---

## 7. Puntos de Integración Futuros (No Implementados)

| Integración | Evidencia de preparación |
|------------|------------------------|
| Pasarela de pago | Payment.Method acepta "Tarjeta" pero sin gateway |
| Facturación electrónica | Entidad Invoice sin API fiscal |
| Impresoras térmicas | Vista Printers referenciada sin backend |
| Proveedores | JS supplier-management.js sin controller |
| Backup cloud | BackupSettings.Schedule sin worker |
| n8n workflows | Deploy separado, sin webhook endpoints en RestBar |
| API pública | Sin API versioning ni documentación OpenAPI |

---

*Análisis de integraciones completo. Sin modificaciones al sistema.*
