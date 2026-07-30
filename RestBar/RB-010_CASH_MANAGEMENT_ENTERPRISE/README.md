# RB-010 — CASH MANAGEMENT ENTERPRISE

**Supreme Edition · Fase 1 Money Operations**  
**Estado:** DISEÑO COMPLETO — NO IMPLEMENTADO

---

## Regla suprema

No construir una caja. Construir un **sistema financiero operativo Enterprise**.

---

## Veredicto diseño

**APTO PARA IMPLEMENTACIÓN FASE A** (pendiente sign-off)

---

## Documentos

| # | Archivo |
|---|---------|
| 01 | [Business Analysis](01_BUSINESS_ANALYSIS.md) |
| 02 | [Enterprise Requirements](02_ENTERPRISE_REQUIREMENTS.md) |
| 03 | [Domain Model](03_DOMAIN_MODEL.md) |
| 04 | [Database Design](04_DATABASE_DESIGN.md) |
| 05 | [Architecture](05_ARCHITECTURE.md) |
| 06 | [Security Model](06_SECURITY_MODEL.md) |
| 07 | [Permission Matrix](07_PERMISSION_MATRIX.md) |
| 08 | [Audit Model](08_AUDIT_MODEL.md) |
| 09 | [Cash Session](09_CASH_SESSION.md) |
| 10 | [Cash Register](10_CASH_REGISTER.md) |
| 11 | [Cash Movements](11_CASH_MOVEMENTS.md) |
| 12 | [Workflows](12_WORKFLOWS.md) |
| 13 | [State Machine](13_STATE_MACHINE.md) |
| 14 | [UX Design](14_UX_DESIGN.md) |
| 15 | [API Design](15_API_DESIGN.md) |
| 16 | [Reports](16_REPORTS.md) |
| 17 | [KPIs](17_KPIS.md) |
| 18 | [Test Plan](18_TEST_PLAN.md) |
| 19 | [Implementation Plan](19_IMPLEMENTATION_PLAN.md) |
| 20 | [Final Certification](20_FINAL_CERTIFICATION.md) |

---

## Decisiones clave

| Decisión | Justificación |
|----------|---------------|
| **Extender Shift**, no reemplazar | Turno laboral ≠ dinero |
| **Payment** sigue verdad de cobro | Certificación 119/119 |
| **CashMovement** ledger inmutable | Auditoría forense |
| **CashSession** aggregate cierre | Industry standard |
| **ICashPaymentHook** | Sin duplicar PaymentController |
| Policy **CashAccess** | Ya en Foundation |

---

## Entidades nuevas

`CashRegister` · `CashSession` · `CashMovement` · `CashCount` · `CashApproval` · `CashIncident` · `CashAuditEvent` · `CashZReport`

## Alteración mínima

`Payment.CashSessionId` · `PaymentRefund.CashSessionId`

---

## Implementación

**6–8 semanas** post-aprobación · Ver [19_IMPLEMENTATION_PLAN.md](19_IMPLEMENTATION_PLAN.md)

**Cero código hasta sign-off.**
