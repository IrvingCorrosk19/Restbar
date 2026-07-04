# Análisis de Causa Raíz (RCA)

## RCA-001 — POS sin productos (DEF-004)

**Impacto:** Bloqueo total del flujo POS  
**Root cause:** ASP.NET Core routing convencional `{controller}/{action}/{id?}` solo enlaza el tercer segmento al parámetro `id`. El método usaba `categoryId`, que quedaba en `Guid.Empty`.

**Evidencia:**
```
[POS] GetProductsByCategory categoryId=00000000-0000-0000-0000-000000000000
```

**Solución:** `[FromRoute(Name = "id")] Guid categoryId`

**Prevención:** Convención de rutas documentada; tests de integración para endpoints con parámetro GUID en URL.

---

## RCA-002 — Audit HTTP 400 (DEF-003)

**Root cause:** El parámetro de acción `action` en `AuditController.Index` colisiona con el token reservado de enrutamiento MVC `{action}`.

**Solución:** Renombrar a `actionFilter` con `[FromQuery(Name = "action")]`.

---

## RCA-003 — Pagos EF/DB mismatch (DEF-006, DEF-007)

**Root cause:** Migración `SecurityHardening` aplicada parcialmente en DB pero `RestBarContext` no mapeaba columnas nuevas. Además, columnas multi-tenant en `payments` usan PascalCase (`BranchId`) no snake_case.

**Solución:** Mapeo explícito en ambas configuraciones `Payment` del contexto.

---

## RCA-004 — Chef sin KDS (DEF-005)

**Root cause:** `OrderController` requiere política `OrderAccess` a nivel de clase; chef no estaba en la lista de roles permitidos.

**Solución:** Ampliar `OrderAccess` + `KitchenAccess` en acciones KDS.

---

## RCA-005 — Contaminación multi-tenant pagos (DEF-008)

**Root cause:** `CreatePartialPayment` tenía guard IDOR pero `GetOrderPaymentSummary` no.

**Solución:** Mismo patrón de validación `BranchId` claim vs `order.BranchId`.

---

## RCA-006 — Órdenes duplicadas por mesa (DEF-009)

**Root cause:** Sin restricción DB para una sola orden activa por mesa; condiciones de carrera en POS.

**Solución:** Índice parcial único `idx_unique_active_order_per_table` + limpieza de datos.
