## Resumen de sesión (30-10-2025)

- **Base de datos (Postgres 18)**:
  - Conexión usada: `Host=localhost;Port=5432;Database=RestBar;Username=postgres;Password=Panama2020$` (appsettings `DefaultConnection`).
  - Migraciones aplicadas correctamente a `RestBar` (30 tablas creadas, incluyendo `orders`, `order_items`, `tables`, `users`, `products`, `audit_logs`, etc.).
  - Verificación con psql desde `C:\Program Files\PostgreSQL\18\bin\psql.exe`.

- **Auditoría**:
  - Middleware: `AuditMiddleware` y `ErrorHandlingMiddleware` activos.
  - Servicio: `AuditLogService` registra usuario, compañía, sucursal, acción, valores viejos/nuevos, IP, user agent, sesión, timestamps UTC.
  - Esquema `AuditLog` mapeado en `RestBarContext` con `timestamp with time zone` y enums registrados.

- **Tracking automático (fechas/usuario)**:
  - `RestBarContext.SaveChanges*(...)` establece `CreatedAt/UpdatedAt` con `DateTime.UtcNow` y `CreatedBy/UpdatedBy` con usuario actual (o "system").
  - Entidades clave configuradas con columnas de auditoría y TZ: `orders`, `order_items`, `payments`, `persons`, `branches`, `companies`, etc.

- **Formato de fechas (Panamá)**:
  - `Program.cs`: Cultura por defecto `es-PA` (dd/MM/yyyy, 24h). Almacenamiento se mantiene en UTC.

- **SignalR (verificación previa)**:
  - Hub `OrderHub` con grupos: `orders`, `kitchen`, `order_{id}`, `table_{id}`, `table_all`.
  - Eventos emitidos: `NewOrder`, `OrderStatusChanged`, `OrderItemStatusChanged`, `OrderCancelled`, `TableStatusChanged`, `KitchenUpdate`.
  - Frontend (`wwwroot/js/order/signalr.js`) escucha y actualiza UI (estados de mesa y orden).

- **Estados de mesa/orden (flujo)**:
  - Mesa: `Disponible → Ocupada → EnPreparacion → ParaPago → Disponible` (según ítems y pago).
  - Orden: creación/envío/cancelación/pago emiten eventos y actualizan mesa.

- **Siembra de datos de prueba**:
  - Endpoint agregado: `POST /Seed/SeedDemoData` (crea compañía, sucursal, áreas, mesas, estación, categorías, productos, admin y orden demo). Permite `[AllowAnonymous]` temporalmente.
  - Alternativa por psql: inserts idempotentes (se usó psql de Postgres 18).

### Comandos útiles (psql)

```bash
"C:\\Program Files\\PostgreSQL\\18\\bin\\psql.exe" "host=localhost port=5432 dbname=RestBar user=postgres password=Panama2020$" -c "SELECT table_name FROM information_schema.tables WHERE table_schema='public' AND table_type='BASE TABLE' ORDER BY table_name;"
```

### Comandos útiles (EF Core)

```powershell
cd C:\Proyectos\RestBar\RestBar\RestBar; dotnet ef database update
```

### Pendientes/Siguientes pasos

- Protecciones: revertir `[AllowAnonymous]` de `SeedController` cuando terminen pruebas.
- Si quieres más datos demo (usuarios/menú/mesas), ampliar `SeedDemoData` o agregar scripts `.sql` en `Scripts/`.


