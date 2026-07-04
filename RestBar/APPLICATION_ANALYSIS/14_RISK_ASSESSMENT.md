# 14 — Risk Assessment

**Sistema:** RestBar  
**Fecha:** 2026-07-04

---

## 1. Metodología

Evaluación de riesgos basada en análisis estático del código, sin ejecución de pruebas. Clasificación:

| Nivel | Criterio |
|-------|----------|
| 🔴 Crítico | Puede causar pérdida financiera, brecha de seguridad o corrupción de datos |
| 🟠 Alto | Puede causar fallo operacional significativo o acceso indebido |
| 🟡 Medio | Degradación de funcionalidad, inconsistencia o deuda técnica con impacto |
| 🟢 Bajo | Mejora recomendada, impacto limitado |

---

## 2. Riesgos de Seguridad

| ID | Nivel | Riesgo | Ubicación | Impacto | Probabilidad |
|----|-------|--------|-----------|---------|-------------|
| SEC-01 | 🔴 | SignalR hub sin autenticación ni validación de groups | Hubs/OrderHub.cs | Exposición de datos operativos en tiempo real | Alta |
| SEC-02 | 🔴 | Endpoints CreateAdmin AllowAnonymous accesibles en producción | AuthController, SeedController | Creación no autorizada de administradores | Media |
| SEC-03 | 🔴 | Credenciales SSH hardcoded en repo | Com/deploy-restbar.ps1 | Compromiso del VPS | Baja pero catastrófica |
| SEC-04 | 🟠 | Password reset token almacenado en columna PasswordHash | AuthService | Bloqueo de login, sobrescritura de hash | Media |
| SEC-05 | 🟠 | Error responses exponen exception.Message | ErrorHandlingMiddleware | Information disclosure | Alta |
| SEC-06 | 🟠 | Cookie SecurePolicy=SameAsRequest | Program.cs | Intercepción de cookie en HTTP | Media |
| SEC-07 | 🟡 | Audit accesible por cualquier usuario autenticado | AuditController | Exposición de logs operativos | Alta |
| SEC-08 | 🟡 | API routes sin PermissionMiddleware | PermissionMiddleware | Bypass de path-based auth | Media |
| SEC-09 | 🟡 | Sin rate limiting en endpoints operacionales | Program.cs | DoS / abuso de API | Baja |
| SEC-10 | 🟡 | DB password en appsettings.Development.json | Config | Exposición en repo | Media |
| SEC-11 | 🟢 | Session registrada pero sin uso | Program.cs | Superficie de ataque innecesaria | Baja |

---

## 3. Riesgos Financieros / POS

| ID | Nivel | Riesgo | Ubicación | Impacto |
|----|-------|--------|-----------|---------|
| FIN-01 | 🔴 | Concurrencia en pagos simultáneos | PaymentController | Doble cobro o inconsistencia (mitigado parcialmente con Version + idempotency) |
| FIN-02 | 🟠 | Void payment puede dejar orden en estado inconsistente | PaymentService.VoidPaymentAsync | Saldo incorrecto, mesa bloqueada |
| FIN-03 | 🟠 | Overpay no bloqueado (solo warning) | PaymentController | Pago excesivo registrado |
| FIN-04 | 🟡 | Sin pasarela de pago externa | PaymentController | Solo pagos manuales (efectivo/tarjeta sin verificación) |
| FIN-05 | 🟡 | Invoice sin flujo UI completo | InvoiceService | Facturación incompleta |
| FIN-06 | 🟢 | Descuentos sin validación contra DiscountPolicy en POS | discounts.js | Descuento arbitrario por usuario |

---

## 4. Riesgos Operacionales

| ID | Nivel | Riesgo | Ubicación | Impacto |
|----|-------|--------|-----------|---------|
| OPS-01 | 🟠 | Navbar "Cocina" apunta a ruta inexistente | _Layout.cshtml | Usuarios no pueden acceder a KDS desde navbar |
| OPS-02 | 🟠 | Productos sin ProductStockAssignment no aparecen en KDS | KitchenService | Ítems enviados a cocina pero invisibles |
| OPS-03 | 🟠 | Mesas fantasma (Ocupada sin orden activa) | TableService | Mesas bloqueadas sin uso |
| OPS-04 | 🟡 | SignalR desconexión deja vistas desactualizadas | signalr.js | Operación con datos stale (mitigado parcialmente con reconnect) |
| OPS-05 | 🟡 | 3 reportes avanzados sin JS implementado | AdvancedReports views | Reportes no funcionales |
| OPS-06 | 🟡 | Vistas de configuración referenciadas sin implementar | AdvancedSettingsController | Navegación a páginas 404 |
| OPS-07 | 🟡 | Dual user management UI (User vs UserManagement) | UserController, UserManagementController | Confusión operativa |
| OPS-08 | 🟢 | Email deshabilitado por defecto | appsettings.json | Sin confirmaciones ni recuperación de password |

---

## 5. Riesgos de Datos / Integridad

| ID | Nivel | Riesgo | Ubicación | Impacto |
|----|-------|--------|-----------|---------|
| DAT-01 | 🟠 | Multi-tenancy solo en aplicación (sin RLS) | Services | Fuga de datos cross-tenant si filtro falla |
| DAT-02 | 🟠 | Enums PG registrados pero columnas usan varchar | RestBarContext | Valores inválidos posibles en DB |
| DAT-03 | 🟡 | Dual category system (categories + product_categories) | Models | Confusión, datos huérfanos |
| DAT-04 | 🟡 | Naming mixto snake_case/PascalCase en DB | Migrations | Errores en SQL manual/reportes |
| DAT-05 | 🟡 | SeedDummyData.sql desactualizado | Scripts/ | Fallo si se ejecuta manualmente |
| DAT-06 | 🟡 | Duplicate fluent config en OnModelCreating | RestBarContext | Comportamiento impredecible |
| DAT-07 | 🟢 | Legacy SHA256 passwords en seed | SeedController | Passwords débiles en demo |

---

## 6. Riesgos de Arquitectura / Técnicos

| ID | Nivel | Riesgo | Ubicación | Impacto |
|----|-------|--------|-----------|---------|
| ARC-01 | 🟠 | Sin background jobs (backup, notificaciones) | — | Backup manual, sin automatización |
| ARC-02 | 🟠 | SignalR sin backplane (single instance) | OrderHub | Sin escalado horizontal |
| ARC-03 | 🟡 | EF Core 9 sobre .NET 8 | RestBar.csproj | Incompatibilidad potencial |
| ARC-04 | 🟡 | OrderService como God Service | OrderService.cs | Mantenibilidad, testing |
| ARC-05 | 🟡 | Sin tests automatizados | — | Regresiones no detectadas |
| ARC-06 | 🟡 | Dependencia de CDNs externos | Views | Funcionalidad degradada si CDN cae |
| ARC-07 | 🟢 | OrderService.cs.backup en repo | Services/ | Confusión para desarrolladores |
| ARC-08 | 🟢 | GenerateJwtTokenAsync naming misleading | AuthService | Confusión (no es JWT real) |

---

## 7. Código Muerto / Componentes Sin Uso

| Componente | Tipo | Ubicación |
|-----------|------|-----------|
| `Views/Payment/Index.cshtml` | Vista huérfana | Duplica PaymentView |
| `accounting.js` | JS sin vista | wwwroot/js/ |
| `supplier-management.js` | JS sin controller | wwwroot/js/supplier/ |
| `inventory-movements.js` | JS no conectado | wwwroot/js/inventory/ |
| `separate-accounts.js` (full) | JS no cargado | wwwroot/js/order/ |
| `OrderService.cs.backup` | Backup obsoleto | Services/ |
| `product_categories` table | Tabla legacy | DB |
| `products.ProductCategoryId` | Columna legacy | DB |
| `AssignmentType` enum | Enum sin columna | Models |
| `Session` middleware | Registrado sin uso | Program.cs |

---

## 8. Duplicidad Detectada

| Elemento | Instancias | Impacto |
|----------|-----------|---------|
| User management UI | User/UserManagement + UserManagement/Index | Confusión |
| Payment UI | Payment/Index + PaymentView/Index | Vista huérfana |
| Category systems | categories + product_categories | Datos duplicados |
| Fluent API config | 7 entidades configuradas 2 veces | Override silencioso |
| SignalR client source | cdnjs (POS) + unpkg (KDS) | Versiones potencialmente distintas |
| User role en claims | Role + UserRole (duplicado) | Redundancia |

---

## 9. Módulos Incompletos

| Módulo | Completitud | Faltante |
|--------|------------|----------|
| POS / Órdenes | 95% | Vistas Details/Create/Edit |
| Pagos | 90% | Pasarela externa |
| KDS | 90% | Link navbar roto |
| Inventario | 80% | inventory-movements.js |
| Reportes avanzados | 70% | 3 JS faltantes, Supplier sin datos |
| Configuración avanzada | 60% | 6+ vistas sin implementar |
| Email | 70% | Sin UI de gestión (Email/Index) |
| Facturación | 30% | Sin UI, sin API fiscal |
| Modificadores | 20% | Sin UI de gestión |
| Clientes/CRM | 20% | Sin UI dedicada |
| Proveedores | 0% | Solo JS huérfano |
| Compras | 0% | No existe |
| Impresoras | 0% | Vista referenciada sin backend |
| Backup automático | 10% | Solo config, sin worker |
| Contabilidad | 10% | Solo JS huérfano |

---

## 10. Matriz de Riesgo Consolidada

| Categoría | Crítico | Alto | Medio | Bajo | Total |
|-----------|---------|------|-------|------|-------|
| Seguridad | 3 | 3 | 5 | 1 | 12 |
| Financiero/POS | 1 | 2 | 3 | 1 | 7 |
| Operacional | 0 | 3 | 5 | 1 | 9 |
| Datos | 0 | 2 | 5 | 1 | 8 |
| Arquitectura | 0 | 2 | 4 | 2 | 8 |
| **Total** | **4** | **12** | **22** | **6** | **44** |

---

## 11. Top 10 Riesgos para Certificación E2E

| Prioridad | ID | Riesgo | Área de prueba E2E |
|-----------|-----|--------|-------------------|
| 1 | FIN-01 | Concurrencia en pagos | Dos pestañas pagando misma orden |
| 2 | SEC-01 | SignalR sin auth | Conectar hub sin login, join groups ajenos |
| 3 | SEC-02 | CreateAdmin en producción | Acceder a /Seed/CreateAdminUser en prod |
| 4 | DAT-01 | Multi-tenant isolation | Usuario branch A accede datos branch B |
| 5 | OPS-01 | Navbar cocina roto | Click "Cocina" en navbar |
| 6 | OPS-02 | KDS sin stock assignment | Producto sin asignación en KDS |
| 7 | FIN-02 | Void payment consistency | Anular pago y verificar estado orden/mesa |
| 8 | OPS-03 | Mesas fantasma | Mesa Ocupada sin orden activa |
| 9 | SEC-07 | Audit acceso amplio | Usuario waiter accede /Audit/Index |
| 10 | OPS-05 | Reportes sin JS | Navegar a CustomerAnalysis, SalesAnalysis |

---

*Evaluación de riesgos por análisis estático. Sin modificaciones al sistema.*
