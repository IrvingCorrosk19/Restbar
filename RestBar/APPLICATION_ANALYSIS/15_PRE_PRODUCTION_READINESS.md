# 15 — Pre-Production Readiness

**Sistema:** RestBar  
**Fecha:** 2026-07-04  
**Propósito:** Evaluar preparación para Certificación Funcional Enterprise y despliegue en producción

---

## 1. Resumen de Preparación

| Área | Estado | Score |
|------|--------|-------|
| Núcleo POS (órdenes, cocina, pagos) | Funcional con riesgos conocidos | 75% |
| Seguridad | Hardening reciente pero gaps críticos | 55% |
| Multi-tenancy | Implementado sin RLS | 65% |
| Reportes | Parcialmente funcional | 60% |
| Configuración | Parcialmente implementada | 50% |
| Infraestructura | Docker + nginx desplegado | 80% |
| Documentación | Análisis completo generado | 85% |
| Tests automatizados | No existen | 0% |
| Módulos periféricos | Mayormente incompletos | 25% |
| **Preparación global estimada** | | **55%** |

---

## 2. Checklist de Preparación por Área

### 2.1 Funcionalidad Core (Requerida para producción)

| # | Requisito | Estado | Evidencia | Acción para certificación |
|---|----------|--------|-----------|--------------------------|
| F01 | Login/logout funcional | ✅ Implementado | AuthController, AuthService | E2E: login por cada rol |
| F02 | Dashboard por rol | ✅ Implementado | HomeController.GetVisibleCardsForRole | E2E: verificar cards por rol |
| F03 | Crear orden en mesa | ✅ Implementado | OrderController, OrderService | E2E: flujo completo |
| F04 | Agregar productos a orden | ✅ Implementado | OrderController.AddItems | E2E: con/sin stock |
| F05 | Enviar a cocina | ✅ Implementado | OrderController.SendToKitchen | E2E: verificar KDS |
| F06 | KDS muestra órdenes | ✅ Implementado | OrderController.StationOrders | E2E: por stationType |
| F07 | Marcar ítems listos | ✅ Implementado | OrderController.MarkItemReady | E2E: chef workflow |
| F08 | Pago parcial | ✅ Implementado | PaymentController.partial | E2E: múltiples pagos |
| F09 | Pago completo | ✅ Implementado | PaymentController.partial | E2E: cierre de orden |
| F10 | Anular pago (void) | ✅ Implementado | PaymentController DELETE | E2E: reversión |
| F11 | Cuentas separadas | ✅ Implementado | PersonController + JS | E2E: split bill |
| F12 | SignalR real-time | ✅ Implementado | OrderHub + clients | E2E: multi-pantalla |
| F13 | Gestión de mesas | ✅ Implementado | TableController | E2E: CRUD + estados |
| F14 | Gestión de productos | ✅ Implementado | ProductController | E2E: CRUD + stock |
| F15 | Gestión de usuarios | ✅ Implementado | UserController | E2E: CRUD + roles |

### 2.2 Seguridad (Requerida para producción)

| # | Requisito | Estado | Gap |
|---|----------|--------|-----|
| S01 | Autenticación cookie | ✅ | — |
| S02 | Password hashing BCrypt | ✅ | Legacy SHA256 en seed |
| S03 | Rate limiting login | ✅ | Solo auth endpoints |
| S04 | CSRF en forms | ⚠️ Parcial | JSON endpoints sin CSRF |
| S05 | Autorización por rol | ✅ | Inconsistencias middleware vs policy |
| S06 | Multi-tenant isolation | ⚠️ Parcial | Sin RLS, filtro en código |
| S07 | SignalR autenticado | ❌ | Hub sin [Authorize] |
| S08 | Seed bloqueado en prod | ⚠️ Parcial | SeedDemoData sí, CreateAdmin no |
| S09 | HTTPS en producción | ✅ | nginx SSL |
| S10 | Auditoría de acciones | ✅ | AuditMiddleware |
| S11 | Idempotencia pagos | ✅ | SecurityHardening migration |
| S12 | Concurrencia órdenes | ✅ | Order.Version |

### 2.3 Infraestructura (Requerida para producción)

| # | Requisito | Estado | Gap |
|---|----------|--------|-----|
| I01 | Docker deployment | ✅ | — |
| I02 | PostgreSQL aislado | ✅ | No expuesto externamente |
| I03 | nginx SSL | ✅ | — |
| I04 | Auto-migrate en startup | ✅ | Swallows failure |
| I05 | DataProtection keys persistentes | ✅ | Docker volume |
| I06 | Health checks | ⚠️ Parcial | Solo postgres healthcheck |
| I07 | Backup automatizado | ❌ | Sin worker |
| I08 | Monitoring/alerting | ❌ | Sin APM ni health endpoint |
| I09 | Log aggregation | ❌ | Solo console + DB audit |

### 2.4 Funcionalidad Deseable (No bloqueante)

| # | Requisito | Estado |
|---|----------|--------|
| D01 | Reportes avanzados completos | ⚠️ 70% |
| D02 | Email transaccional | ⚠️ Implementado, deshabilitado |
| D03 | Facturación electrónica | ❌ |
| D04 | Pasarela de pago | ❌ |
| D05 | Impresoras térmicas | ❌ |
| D06 | Gestión de proveedores | ❌ |
| D07 | Módulo de compras | ❌ |
| D08 | Modificadores de producto UI | ❌ |
| D09 | CRM/Clientes UI | ❌ |
| D10 | Backup automático | ❌ |

---

## 3. Criterios de Aceptación para Certificación

### 3.1 Bloqueantes (Must Pass)

| ID | Criterio | Módulo |
|----|----------|--------|
| BP01 | Login exitoso para los 11 roles | Auth |
| BP02 | Usuario solo ve módulos de su rol | Dashboard + Permissions |
| BP03 | Flujo completo: mesa → orden → cocina → pago → cierre | POS end-to-end |
| BP04 | Pago parcial + pago final = total correcto | Payments |
| BP05 | Void payment revierte correctamente | Payments |
| BP06 | Dos pagos simultáneos no causan doble cobro | Concurrency |
| BP07 | Datos aislados por sucursal (no cross-branch) | Multi-tenant |
| BP08 | KDS actualiza en tiempo real al enviar a cocina | SignalR |
| BP09 | Stock decrementa al enviar a cocina | Inventory |
| BP10 | Mesa se libera al completar orden | Tables |
| BP11 | Seed/CreateAdmin no accesible en producción | Security |
| BP12 | Auditoría registra acciones críticas | Audit |

### 3.2 Importantes (Should Pass)

| ID | Criterio | Módulo |
|----|----------|--------|
| SP01 | Split bill funciona con múltiples personas | Person |
| SP02 | Descuento se aplica correctamente | Discounts |
| SP03 | Reporte de ventas con filtros de fecha | Reports |
| SP04 | CRUD de productos con categorías | Products |
| SP05 | Asignación stock-estación afecta KDS | Stock Assignment |
| SP06 | Liberar mesas fantasma funciona | Tables |
| SP07 | SuperAdmin gestiona companies/branches | SuperAdmin |
| SP08 | Configuración de impuestos afecta cálculos | TaxRate |
| SP09 | Usuario inactivo no puede login | Auth |
| SP10 | Rate limiter bloquea brute force | Auth |

### 3.3 Deseables (Nice to Have)

| ID | Criterio | Módulo |
|----|----------|--------|
| NP01 | Reportes avanzados cargan sin error JS | Advanced Reports |
| NP02 | Email de confirmación se envía | Email |
| NP03 | Navbar "Cocina" funciona | Navigation |
| NP04 | Inventario muestra alertas stock bajo | Inventory |
| NP05 | Recuperación de contraseña funciona | Auth |

---

## 4. Plan de Certificación Funcional Recomendado

### Fase 1: Smoke Tests (1-2 días)
- Login/logout por rol
- Dashboard cards visibility
- Navegación a cada módulo visible

### Fase 2: Core POS (3-5 días)
- Flujo completo mesa→orden→cocina→pago
- Pagos parciales, completos, void
- Split bill
- Concurrencia (2 pestañas)
- SignalR multi-pantalla

### Fase 3: Seguridad y Multi-Tenant (2-3 días)
- Aislamiento por branch
- Permisos por rol (acceso denegado)
- Endpoints públicos en producción
- SignalR sin autenticación
- IDOR en API endpoints

### Fase 4: Administración (2-3 días)
- CRUD productos, categorías, mesas, áreas, estaciones
- CRUD usuarios y asignaciones
- Configuración (tax, currency, settings)
- SuperAdmin flows

### Fase 5: Reportes y Periféricos (1-2 días)
- Reportes básicos y avanzados
- Inventario y stock
- Email (si habilitado)
- Módulos incompletos (documentar como known issues)

---

## 5. Entorno de Certificación Recomendado

| Aspecto | Recomendación |
|---------|--------------|
| Base de datos | PostgreSQL limpio + SeedDemoData |
| Usuarios | 9 roles del seed (password: 123456) |
| Entorno | Development local o staging en VPS |
| Herramientas | Browser manual + posible Playwright/Cypress |
| Datos | Compañía "RestBar Principal", Branch "RestBar Centro" |
| Multi-branch | Crear segunda sucursal para test de aislamiento |

---

## 6. Known Issues Aceptables para Producción

| Issue | Justificación | Workaround |
|-------|--------------|------------|
| Sin pasarela de pago | POS manual es válido para restaurantes | Operación con efectivo/tarjeta manual |
| Email deshabilitado | No bloquea operación | Habilitar cuando SMTP esté configurado |
| Reportes avanzados parciales | No bloquea operación diaria | Usar reportes básicos |
| Sin backup automático | Riesgo mitigable | Backup manual de PostgreSQL |
| Módulos periféricos ausentes | No son core POS | Planificar en roadmap |

---

## 7. Known Issues NO Aceptables para Producción

| Issue | Riesgo | Requerido antes de prod |
|-------|--------|------------------------|
| SEC-01: SignalR sin auth | Exposición de datos | Sí |
| SEC-02: CreateAdmin anónimo | Escalación de privilegios | Sí |
| SEC-03: SSH creds en repo | Compromiso de VPS | Sí |
| OPS-01: Navbar cocina roto | Operación de cocina | Sí (workaround: usar dashboard card) |
| FIN-01: Concurrencia pagos | Pérdida financiera | Validar en E2E |

---

## 8. Veredicto Preliminar

| Pregunta | Respuesta |
|----------|----------|
| ¿El núcleo POS está listo para certificación? | **Sí**, con riesgos documentados |
| ¿El sistema está listo para producción? | **No sin remedición** de 4 issues críticos |
| ¿Se puede iniciar certificación funcional? | **Sí**, con el plan de 5 fases propuesto |
| ¿Cuánto tiempo estimado de certificación? | 10-15 días de pruebas manuales sistemáticas |

---

*Evaluación de preparación pre-producción. Sin modificaciones al sistema.*
