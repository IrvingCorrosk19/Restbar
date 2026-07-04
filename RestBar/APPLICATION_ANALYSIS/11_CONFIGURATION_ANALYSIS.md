# 11 — Configuration Analysis

**Sistema:** RestBar  
**Fecha:** 2026-07-04

---

## 1. Archivos de Configuración

| Archivo | Entorno | Propósito |
|---------|---------|-----------|
| `appsettings.json` | Base (todos) | Configuración por defecto |
| `appsettings.Development.json` | Development | Overrides locales |
| `appsettings.Production.json` | Production | Overrides producción |
| `Com/restbar/.env.example` | Docker | Variables de entorno PostgreSQL |
| `docker-compose.yml` | Docker | Orquestación de servicios |
| `Com/restbar/docker-compose.yml` | VPS | Variante producción con forwarded headers |

---

## 2. appsettings.json (Base)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=RestBar;Username=postgres;Password="
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Email": {
    "Enabled": false,
    "Smtp": { "Host": "smtp.gmail.com", "Port": 587, "UseSsl": false, "StartTls": true },
    "From": { "Address": "", "Name": "RestBar" },
    "BaseUrl": "http://localhost:5001"
  },
  "RateLimit": {
    "AuthEndpoints": { "PermitLimit": 5, "WindowSeconds": 60 }
  }
}
```

### Parámetros Clave

| Sección | Parámetro | Valor default | Notas |
|---------|-----------|--------------|-------|
| ConnectionStrings | DefaultConnection | localhost PG | Password vacío en base |
| Email | Enabled | false | Email deshabilitado por defecto |
| Email.Smtp | Host | smtp.gmail.com:587 | Gmail template |
| Email | BaseUrl | localhost:5001 | Para links en emails |
| AllowedHosts | * | Cualquier host | Sin restricción |
| RateLimit | AuthEndpoints | 5/60s | ⚠ Hardcoded en Program.cs, no lee config |

---

## 3. appsettings.Development.json

| Parámetro | Valor | Riesgo |
|-----------|-------|--------|
| DefaultConnection | Host=localhost;Password=Panama2020$ | ⚠ Password en archivo |
| Logging | Debug/Information | Normal para dev |

---

## 4. appsettings.Production.json

| Parámetro | Valor | Notas |
|-----------|-------|-------|
| Logging | Warning (incl. EF Core) | Reduce verbosidad |
| Email.BaseUrl | https://restbar.autonomousflow.lat | URL producción |
| ConnectionStrings | (no definido) | Via env var `ConnectionStrings__DefaultConnection` |

---

## 5. Variables de Entorno (Docker/VPS)

| Variable | Fuente | Propósito |
|----------|--------|-----------|
| `POSTGRES_DB` | .env | Nombre de base de datos |
| `POSTGRES_USER` | .env | Usuario PostgreSQL |
| `POSTGRES_PASSWORD` | .env | Password PostgreSQL |
| `ConnectionStrings__DefaultConnection` | docker-compose | Connection string completo |
| `ASPNETCORE_ENVIRONMENT` | docker-compose | Production |
| `ASPNETCORE_URLS` | docker-compose | http://+:8080 |
| `ASPNETCORE_FORWARDEDHEADERS_ENABLED` | VPS compose | true (detrás de nginx) |

---

## 6. Configuración en Base de Datos

Además de appsettings, el sistema almacena configuración operativa en tablas DB (scope por CompanyId):

### 6.1 SystemSettings (Key-Value)

| Setting (ejemplo seed) | Valor | Propósito |
|------------------------|-------|-----------|
| RestaurantName | RestBar Principal | Nombre del negocio |
| TimeZone | America/Panama | Zona horaria |
| Language | es | Idioma |
| DateFormat | dd/MM/yyyy | Formato fecha |
| Currency | USD | Moneda por defecto |

**Servicio:** `SystemSettingsService` — CRUD por CompanyId.

### 6.2 Currency

| Campo | Descripción |
|-------|-------------|
| Code | ISO (USD, PAB) |
| Name | Nombre completo |
| Symbol | $ |
| ExchangeRate | Tasa de cambio |
| IsDefault | Moneda principal |

### 6.3 TaxRate

| Campo | Descripción |
|-------|-------------|
| Name | Nombre del impuesto (ITBMS, etc.) |
| Rate | Porcentaje |
| IsDefault | Impuesto por defecto |

### 6.4 DiscountPolicy

| Campo | Descripción |
|-------|-------------|
| Name | Nombre de la política |
| Type | Percentage / FixedAmount |
| Value | Valor del descuento |
| IsActive | Habilitada/deshabilitada |

### 6.5 OperatingHours

| Campo | Descripción |
|-------|-------------|
| DayOfWeek | 0-6 (Domingo-Sábado) |
| OpenTime | Hora apertura |
| CloseTime | Hora cierre |
| IsClosed | Día cerrado |

### 6.6 NotificationSettings

| Tipo | Descripción |
|------|-------------|
| OrderConfirmation | Email al completar pago |
| PasswordRecovery | Email de recuperación |
| KitchenNotification | Notificación a cocina |
| LowStockAlert | Alerta stock bajo |

### 6.7 BackupSettings

| Campo | Descripción |
|-------|-------------|
| Schedule | Cron string (ej: "0 2 * * *") |
| LastBackup | Timestamp último backup |
| IsEnabled | Habilitado |

**⚠ Sin worker:** El schedule se almacena pero no se ejecuta automáticamente.

### 6.8 EmailTemplate

| Template (seed) | Propósito |
|--------------|-----------|
| OrderConfirmation | Confirmación de pedido |
| PasswordRecovery | Recuperación de contraseña |
| Welcome | Bienvenida de usuario |

---

## 7. Configuración por Sucursal vs Global

| Configuración | Scope | Tabla |
|--------------|-------|-------|
| SystemSettings | Company | SystemSettings |
| Currency, Tax, Discount | Company | Currencies, TaxRates, DiscountPolicies |
| OperatingHours | Company | OperatingHours |
| NotificationSettings | Company | NotificationSettings |
| BackupSettings | Company | BackupSettings |
| EmailTemplates | Company | email_templates |
| Products, Categories | Company + Branch | products, categories |
| Orders, Payments | Company + Branch | orders, payments |
| Users | Branch | users |
| Areas, Tables, Stations | Company + Branch | areas, tables, stations |

---

## 8. Configuración de Cultura (Hardcoded)

```csharp
// Program.cs
var panamaCulture = new CultureInfo("es-PA");
panamaCulture.DateTimeFormat.ShortDatePattern = "dd/MM/yyyy";
CultureInfo.DefaultThreadCurrentCulture = panamaCulture;
```

No es configurable via appsettings ni DB.

---

## 9. Configuración de Autenticación (Hardcoded)

| Parámetro | Valor | Ubicación |
|-----------|-------|-----------|
| Cookie name | RestBarAuth | Program.cs |
| Session duration | 8 horas sliding | Program.cs |
| Session idle timeout | 30 min | Program.cs |
| Rate limit auth | 5 req/60s/IP | Program.cs (hardcoded) |
| BCrypt work factor | 12 | AuthService.cs |
| Login path | /Auth/Login | Program.cs |
| Default route | Auth/Login | Program.cs |

---

## 10. Configuración de SignalR

| Parámetro | Valor |
|-----------|-------|
| Hub path | /orderHub |
| Groups | Definidos en OrderHub.cs (no configurable) |
| Client library | CDN (no version pinned en KDS) |

---

## 11. Configuración de Infraestructura (Docker)

| Servicio | Imagen | Puerto | Volumen |
|----------|--------|--------|---------|
| restbar_web | Build local (Dockerfile) | 8080→8084 | dataprotection-keys |
| restbar_postgres | postgres:15 | (interno) | postgres_data |

| Red | Tipo |
|-----|------|
| restbar_net | bridge (aislada) |

---

## 12. Configuración nginx (VPS)

| Parámetro | Valor |
|-----------|-------|
| Dominio | restbar.autonomousflow.lat |
| SSL | Let's Encrypt |
| Proxy pass | 127.0.0.1:8084 |
| WebSocket | Upgrade headers habilitados |
| HTTP redirect | → HTTPS |

---

## 13. Impacto de Configuración en Operación

| Cambio | Impacto |
|--------|---------|
| TaxRate | Afecta cálculos de impuesto en órdenes/pagos |
| DiscountPolicy | Afecta descuentos aplicables |
| Currency | Afecta formato de moneda en UI |
| Email:Enabled | Habilita/deshabilita toda comunicación email |
| NotificationSettings | Controla qué emails se envían |
| OperatingHours | Almacenado pero sin enforcement en código |
| AllowNegativeStock (producto) | Permite venta sin stock |
| ProductStockAssignment | Determina visibilidad en KDS |

---

## 14. Gaps de Configuración

| Gap | Detalle |
|-----|---------|
| Sin feature flags | No hay toggle de funcionalidades |
| Sin config por ambiente en DB | Misma config para dev/prod en DB |
| Rate limit hardcoded | No lee appsettings.RateLimit |
| Sin config de impresora | Vista referenciada sin backend |
| Sin config de pasarela de pago | Pagos solo internos |
| OperatingHours sin enforcement | Se almacena pero no se valida al crear órdenes |
| Sin config de timeout SignalR | Defaults de SignalR |

---

*Análisis de configuración completo. Sin modificaciones al sistema.*
