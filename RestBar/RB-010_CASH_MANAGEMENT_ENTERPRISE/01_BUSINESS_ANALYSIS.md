# 01 — BUSINESS ANALYSIS

**RB-010:** Cash Management Supreme Edition  
**Fase:** Diseño únicamente — NO implementar hasta certificación de diseño  
**Fecha:** 2026-07-29

---

# 1. Problema de negocio

RestBar certifica **POS + pagos** (119/119) pero **no administra el dinero físico**. Consecuencias medidas en auditorías:

| Dolor | Impacto |
|-------|---------|
| Sin cierre de caja | Excel paralelo, fraude, errores no detectados |
| Sin arqueo | Faltantes/sobrantes invisibles hasta fin de mes |
| Pagos sin sesión de caja | Imposible atribuir responsabilidad al cajero |
| Sin Z-report | Contador y dueño no confían en el sistema |
| Propinas sin liquidación | Conflictos laborales |

**Regla suprema:** ¿Más control, menos pérdidas, más confianza 10 años? Si no → no entra.

---

# 2. Cómo funciona una caja profesional (mejores prácticas extraídas)

## QSR / cadena (McDonald's, Starbucks, Domino's)

- **Un drawer por estación/caja** con fondo fijo (par level)
- Apertura con conteo denominaciones + supervisor witness (opcional)
- Todo cobro **atribuido a drawer + cashier + business day**
- Retiros (paid-outs) y reposiciones (pay-ins) con motivo y aprobación
- Cierre: conteo físico vs expected; diferencia documentada
- **Blind close** opcional (cajero no ve expected hasta después de contar)
- Z-report por business day + por drawer
- Segregación: cajero cobra; supervisor aprueba void/refund/over threshold

## Full-service (Applebee's, Chili's, Olive Garden, Texas Roadhouse)

- Mesero cobra o cajero central; **turno + drawer** o **server banking**
- Propinas en tránsito vs propinas declaradas en cierre
- Pagos mixtos desglosados por método
- Cierre parcial en cambio de turno (mid-shift drop)
- Manager override en descuentos/refunds

## Enterprise POS (Oracle, Toast, NCR, Micros, Lightspeed, Square)

| Capacidad table-stakes | RestBar hoy |
|------------------------|-------------|
| Cash drawer open/close | ❌ |
| Starting bank / float | ❌ |
| Paid-in / paid-out | ❌ |
| Blind count | ❌ |
| Expected vs actual | ❌ |
| Z/X report | ❌ |
| Payment → drawer link | ❌ (Payment existe sin CashSession) |
| Void/refund audit trail | ⚠️ PaymentRefund parcial |
| Tips settlement | ⚠️ TipAllocation sin UI cierre |
| Multi-drawer per branch | ❌ |
| Role segregation | ⚠️ Policies existen; no cash-specific |

---

# 3. Qué NO copiar de competidores

- Hardware lock específico NCR (adapter después)
- Payroll integrado Toast
- Marketplace apps Square

## Qué sí adoptar como estándar de industria

1. **Ledger inmutable** de movimientos de caja  
2. **Expected cash** calculado desde Payments + movements  
3. **Session aggregate** (CashSession) como unidad de cierre  
4. **Register** como recurso multitenant por Branch  
5. **Blind count + supervisor approval** en varianza > umbral  
6. **Z-report** exportable contabilidad  

## Diferenciador RestBar LATAM

- Multitenant nativo (Company/Branch) certificado 51/51  
- Integración profunda POS→Payment→Cash **sin reimplementar cobros**  
- Auditoría forense + Command Center alertas en español  
- Yappy/ACH como métodos de pago first-class (Panamá/LATAM)  
- Trazabilidad mesero+cajero+shift en un solo sistema  

---

# 4. Alcance RB-010 v1 (Fase 1 Money Ops)

### In scope v1
Apertura, operación, movimientos manuales, vinculación Payment, cierre, arqueo, Z-report, auditoría, reportes core, Command Center widgets, tests MT.

### Out of scope v1 (diseñar hooks, implementar después)
- Precuenta/fiscal (Invoice extend)  
- Depósito bancario automatizado  
- Caja móvil offline  
- Franquicia royalty desde caja  
- Integración contable ERP  

---

# 5. Actores

| Actor | Job-to-be-done |
|-------|----------------|
| Cajero | Abrir, cobrar (via POS existente), retiros menores, cerrar |
| Supervisor | Aprobar varianzas, overrides, revisar incidentes |
| Gerente | Configurar registers, fondos, umbrales, reapertura |
| Contador | Export Z, conciliar métodos no efectivo |
| Auditor | Trail completo, fraude patterns |
| Dueño/Holding | KPI integridad caja multi-sucursal |

---

# 6. Relación con building blocks existentes

| Existente | Rol en Cash |
|-----------|-------------|
| `Shift` | Turno laboral mesero/cajero — **NO reemplazar** |
| `Payment` | Verdad de cobros — Cash **referencia**, no duplica |
| `PaymentRefund` | Reembolsos — genera CashMovement negativo |
| `TipAllocation` | Propinas — liquidación en cierre sesión |
| `Invoice` | Fuera de cash v1 (precuenta F1.2) |
| `AuditLog` | Eventos HTTP — CashAuditEvent complementa |
| `OrderHub` | Alertas caja abierta, varianza |
| `TenantScope` | Aislamiento obligatorio |
| Policy `CashAccess` | Ya registrada en Foundation |

---

# 7. Métricas de éxito del módulo

| KPI | Target piloto 90 días |
|-----|----------------------|
| Cash Variance | < 0.5% ventas efectivo |
| Cierres sin Excel | 100% |
| Tiempo cierre | < 5 min |
| Incidentes no documentados | 0 |
| IDOR cross-tenant | 0 |
| Certificación RB-010 | 100% PASS |
